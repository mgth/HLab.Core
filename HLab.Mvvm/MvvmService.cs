using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HLab.Base;
using HLab.Core.Annotations;
using HLab.Mvvm.Annotations;

namespace HLab.Mvvm;

public class MvvmService : IMvvmService
{
   public class Bootloader(IMvvmService mvvm) : Core.Annotations.Bootloader
   {
      public override Task<BootState> LoadAsync()
      {
         if (!mvvm.IsPlatformRegistered) return Task.FromResult(BootState.Requeue);

         mvvm.Register();
         return base.LoadAsync();
      }
   }


   readonly IMessagesService _messageBus;
   IMvvmPlatformImpl? _platform;

   readonly ConcurrentDictionary<Type, MvvmBaseEntry> _entries = new();

   /// <summary>
   /// Memoized lookups : resolution climbs type and view-mode hierarchies, the
   /// result never changes once every assembly is registered. Cleared on Register.
   /// </summary>
   readonly ConcurrentDictionary<(Type getType, Type viewMode, Type viewClass), Type?> _linkedTypeCache = new();

   public MvvmService(
       IMessagesService messageBus,
       Func<Type, object> locateFunc
       )
   {
      LocateFunc = locateFunc;
      _messageBus = messageBus;
      ServiceState = ServiceState.NotConfigured;
      MainContext = new MvvmContext(null, "root", this);
   }

   public Func<Type, object> LocateFunc { get; }

   public void RegisterPlatform<T>() where T : IMvvmPlatformImpl
   {
      _platform = (IMvvmPlatformImpl)LocateFunc(typeof(T));
   }

   public bool IsPlatformRegistered => _platform != null;

   /// <summary>
   /// Ensures that the platform is registered before performing operations that depend on it.
   /// </summary>
   /// <exception cref="InvalidOperationException">
   /// Thrown when the platform is not registered.
   /// </exception>
   [MemberNotNull(nameof(_platform))]
   void CheckPlatformRegistered()
   {
      if (_platform == null)
      {
         throw new InvalidOperationException("Platform not registered");
      }
   }

   /// <summary>
   /// The main context of the application
   /// </summary>
   /// <remarks>
   /// This is the context used to register all views and viewmodels
   /// </remarks>
   public IMvvmContext MainContext { get; }

   public HelperFactory<IViewHelper> ViewHelperFactory { get; } = new();

   readonly ConcurrentDictionary<Type, byte> _platformRegisteredRuntimeTypes = new();

   public Type? GetLinkedType(Type getType, Type viewMode, Type viewClass)
   {
      var runtimeType = getType;

      if (getType.IsConstructedGenericType)
      {
         getType = getType.GetGenericTypeDefinition();
      }

      var linked = _linkedTypeCache.GetOrAdd((getType, viewMode, viewClass),
         k => ResolveLinkedType(k.getType, k.viewMode, k.viewClass));

      // Les génériques fermés au runtime (ex : EntityFilterNullable<Country>) ne sont
      // connus qu'ici : la plateforme doit les enregistrer aussi (WPF résout ses
      // DataTemplates implicites par type fermé, jamais par définition ouverte).
      if (linked != null && runtimeType != getType && _platform != null
          && _platformRegisteredRuntimeTypes.TryAdd(runtimeType, 0))
      {
         _platform.Register(runtimeType);
      }

      return linked;
   }

   Type? ResolveLinkedType(Type getType, Type viewMode, Type viewClass)
   {
      // exact registration
      if (_entries.TryGetValue(getType, out var best))
      {
         var result = best.GetLinked(viewClass, viewMode);
         if (result.LinkedType != null)
            return result.LinkedType;
      }

      // closest registered base type or interface
      var level = int.MaxValue;
      Type? linkedType = null;
      foreach (var entry in _entries.Where(e => e.Key.IsAssignableFrom(getType)))
      {
         var t = getType;
         var l = 0;
         while (t.BaseType != null && entry.Key.IsAssignableFrom(t)) { t = t.BaseType; l++; }

         if (l >= level) continue;

         var lt = entry.Value.GetLinked(viewClass, viewMode).LinkedType;
         if (lt == null) continue;

         linkedType = lt;
         level = l;
      }

      if (linkedType != null) return linkedType;

      // fall back to the parent view mode
      var baseMode = viewMode.BaseType;
      if (baseMode == typeof(ViewMode) || baseMode == null) return null;

      return ResolveLinkedType(getType, baseMode, viewClass);
   }

   /// <summary>
   /// Register views from every loaded assembly that can contain some : an
   /// assembly implementing IView necessarily references HLab.Mvvm.Annotations.
   /// </summary>
   public virtual void Register()
   {
      CheckPlatformRegistered();

      _platform.Register(this);

      var assemblies = AssemblyHelper.GetReferencingAssemblies(typeof(IView)).ToList();
      _perAssemblyProgress = 1.0 / Math.Max(1, assemblies.Count);
      foreach (var assembly in assemblies)
      {
         Register(assembly);
      }

      ServiceState = ServiceState.Available;
   }

   double _perAssemblyProgress = 0.0;
   double _progress = 0.0;

   void Register(Assembly assembly)
   {
      // Find all views and register it
      var views = assembly.GetTypesSafe().Where(t => typeof(IView).IsAssignableFrom(t)).ToList();

      if (views.Count == 0)
      {
         _progress += _perAssemblyProgress;
         OnProgress(_progress, assembly.GetName()?.Name ?? "");
      }
      else
      {
         var perViewProgress = _perAssemblyProgress / views.Count;

         foreach (var viewType in views)
         {
            OnProgress(_progress, viewType.Name);

            foreach (var t in viewType.GetInterfaces().Where(i =>
                         i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(IView<,>)
                     ))
            {
               var viewMode = t.GetGenericArguments()[0];
               var baseType = t.GetGenericArguments()[1];
               var viewClasses = GetViewClasses(viewType).ToList();

               OnProgress(_progress,
                   viewType.Name + " - " + viewMode.Name + " - " + baseType.Name + " - " + viewClasses.Count);

               if (viewClasses.Count > 0)
               {
                  foreach (var cls in viewClasses)
                     RegisterWithImplementations(baseType, viewType, cls, viewMode);
               }
               else
               {
                  RegisterWithImplementations(baseType, viewType, typeof(IDefaultViewClass), viewMode);
               }
            }

            _progress += perViewProgress;
         }
      }
   }

   /// <summary>
   /// Candidate view-model implementations : concrete types from the assemblies
   /// that reference HLab.Mvvm.Annotations, excluding design-time view models.
   /// </summary>
   List<Type>? _candidateTypes;
   List<Type> CandidateTypes => _candidateTypes ??= AssemblyHelper
      .GetReferencingAssemblies(typeof(IView))
      .SelectMany(a => a.GetTypesSafe())
      .Where(t => !t.IsAbstract && !typeof(IDesignViewModel).IsAssignableFrom(t))
      .ToList();

   /// <summary>
   /// Register a view for its declared type, plus the concrete implementations
   /// of that type : a view declared against a view-model interface or base
   /// class only reaches the model through the IViewModel&lt;TModel&gt; chain of
   /// the concrete view models, which must be registered from the
   /// implementations themselves.
   /// </summary>
   void RegisterWithImplementations(Type baseType, Type linkedType, Type viewClass, Type viewMode)
   {
      Register(baseType, linkedType, viewClass, viewMode);

      foreach (var t in CandidateTypes.Where(t => baseType.IsAssignableFrom(t) && !t.IsAssignableFrom(baseType)))
         Register(t, linkedType, viewClass, viewMode);
   }

   public void Register(Type baseType, Type linkedType, Type viewClass, Type viewMode)
   {
      CheckPlatformRegistered();

      // resolution results may change once a new link is known
      _linkedTypeCache.Clear();

      var type = baseType;

      while (true)
      {
         if (!viewClass.IsInterface) throw new ArgumentOutOfRangeException("viewClass must be interface " + viewClass.Name);

         // IDesignViewModel are to be used at design time only
         if (typeof(IDesignViewModel).IsAssignableFrom(linkedType)) return;

         Debug.WriteLine(type?.Name + "->" + linkedType.Name + ":" + viewClass.Name + "#" + viewMode.Name);
         Debug.Assert(type != linkedType);
         Debug.Assert(type != null, nameof(type) + " != null");

         var entry = _entries.GetOrAdd(type, (t) =>
         {
            _platform.Register(t);
            return new MvvmBaseEntry(t);
         });

         entry.Register(linkedType, viewClass, viewMode);

         if (!GetModelType(type, out var modelType)) break;

         linkedType = type;
         type = modelType;
      }
   }

   public IView GetNotFoundView(Type getType, Type viewMode, Type viewClass)
   {
      CheckPlatformRegistered();
      return _platform.GetNotFoundView(getType, viewMode, viewClass);
   }

   public void PrepareView(IView view)
   {
      CheckPlatformRegistered();
      _platform.PrepareView(view);
   }

   public IWindow ViewAsWindow(IView? view)
   {
      CheckPlatformRegistered();
      return _platform.ViewAsWindow(view);
   }

   public IWindow ViewAsWindow<T>(IView? view) where T: IWindow, new()
   {
      CheckPlatformRegistered();
      return _platform.ViewAsWindow<T>(view);
   }

   /// <summary>
   /// Get all ViewClass assigned to a specific type
   /// </summary>
   static IEnumerable<Type> GetViewClasses(Type type)
        => type
            .GetInterfaces()
            .Where(i => typeof(IViewClass).IsAssignableFrom(i) && typeof(IViewClass) != i);


   readonly ConcurrentDictionary<Type, Type?> _modelsTypes = new();

   /// <summary>
   /// return Model type from IViewModel&lt;TModel&gt;
   /// </summary>
   bool GetModelType(Type type, out Type? modelType)
   {
      modelType = _modelsTypes.GetOrAdd(type, (t) =>
      {
         foreach (var @interface in t.GetInterfaces())
         {
            if (!@interface.IsGenericType) continue;

            if (@interface.GetGenericTypeDefinition() != typeof(IViewModel<>)) continue;

            if (@interface.GenericTypeArguments.Length <= 0) continue;

            var modelType = @interface.GenericTypeArguments[0];
            Debug.Assert(modelType != type);
            return modelType;
         }
         return null;
      });

      return modelType != null;
   }

   protected virtual void OnProgress(double progress, string text)
   {
      _messageBus.Publish(new ProgressMessage(progress, text));
   }

   internal class MvvmBaseEntry
   {
      public Type BaseType { get; }
      public MvvmBaseEntry(Type baseType)
      {
         BaseType = baseType;
      }

      /// <summary>
      /// All mvvm entries linked to this based type
      /// </summary>
      readonly HashSet<MvvmLinkedEntry> _list = new();


      public void Register(Type linkedType, Type viewClass, Type viewMode)
      {
         lock (_list)
         {
            _list.Add(new MvvmLinkedEntry(linkedType, viewClass, viewMode));
         }
      }

      public MvvmLinkedEntry GetLinked(Type? viewClass, Type? viewMode)
      {
         viewClass ??= typeof(IDefaultViewClass);
         viewMode ??= typeof(DefaultViewMode);

         var result = new MvvmLinkedEntry();
         lock (_list)
         {
            foreach (var e in _list
                         .Where(e => e.ViewClass.IsAssignableFrom(viewClass))
                         .Where(e => e.ViewMode.IsAssignableFrom(viewMode)))
            {
               if (result.LinkedType == null) { result = e; continue; } // initial

               if (result.ViewClass != null && result.ViewClass.IsAssignableFrom(e.ViewClass)) { result = e; continue; } // better viewClass

               // TODO : deal with better ViewMode
            }
         }

         return result;
      }
   }

   internal readonly struct MvvmLinkedEntry
   {
      public Type LinkedType { get; }
      public Type? ViewClass { get; }
      public Type? ViewMode { get; }
      public bool Cacheable { get; }

      public override string ToString() => LinkedType.Name + "(" + ViewClass?.Name + ":" + ViewMode?.Name + ")";

      public MvvmLinkedEntry(Type linkedType, Type viewClass, Type viewMode)
      {
         LinkedType = linkedType;
         ViewClass = viewClass;
         ViewMode = viewMode;
         var attr = LinkedType.GetCustomAttribute<MvvmCacheabilityAttribute>();
         Cacheable = (attr == null || attr.Cacheability == MvvmCacheability.Cacheable);
      }

      public override bool Equals(object? other)
      {
         if (other is MvvmLinkedEntry e)
         {
            return EqualityComparer<Type>.Default.Equals(LinkedType, e.LinkedType) &&
                   EqualityComparer<Type>.Default.Equals(ViewClass, e.ViewClass) &&
                   EqualityComparer<Type>.Default.Equals(ViewMode, e.ViewMode);
         }
         return false;
      }

      public override int GetHashCode()
      {
         return System.HashCode.Combine(LinkedType, ViewClass, ViewMode);
      }

      public static bool operator ==(MvvmLinkedEntry c1, MvvmLinkedEntry c2)
      {
         return c1.Equals(c2);
      }

      public static bool operator !=(MvvmLinkedEntry c1, MvvmLinkedEntry c2)
      {
         return !c1.Equals(c2);
      }
   }

   public ServiceState ServiceState { get; private set; }
}
