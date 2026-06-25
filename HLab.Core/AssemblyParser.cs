using HLab.Base;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HLab.Core;

public class AssemblyParser
{
   Assembly[] _assemblies;

   public void AddAssembliesReferencing<T>()
   {
      var assemblies = AssemblyHelper.GetReferencingAssemblies(typeof(T).Assembly).ToArray();
      _assemblies = _assemblies == null ? assemblies : _assemblies.Union(assemblies).ToArray();
   }

   struct Parser
   {
      public Func<Type, bool> Condition;
      public Action<Type> Action;
   }

   readonly List<Parser> _parsers = new();

   public void AddParser(Func<Type, bool> condition, Action<Type> action) => _parsers.Add(new Parser { Condition = condition, Action = action });

   public void Add<T>(Action<Type> action)
   {
      AddAssembliesReferencing<T>();
      AddParser(t => t.IsAssignableTo(typeof(T)), action);
   }

   public void Parse()
   {
      var assemblies = AssemblyHelper.GetAssemblies().Where(a => a.GetName().FullName.Contains("HLab.Erp.Lims")).ToArray();
      if (_assemblies == null) return;
      foreach (var assembly in _assemblies) Parse(assembly);
   }

   void Parse(Assembly assembly)
   {
      var types = assembly.ExportedTypes.Where(t => !t.IsAbstract);
      //var types = assembly.GetTypes().Where(t => !t.IsAbstract);
      foreach (var type in types) Parse(type);
   }

   void Parse(Type type)
   {
      foreach (var parser in _parsers)
      {
         if (parser.Condition(type)) parser.Action(type);
      }
   }
   
   public void LoadReferencedAssemblies(string name)
   {
      
      var assemblies = new List<string>();
      HashSet<string?> loadedAssemblies = [.. AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name)];
      
      var referencedAssemblies = Assembly.GetEntryAssembly()?.GetReferencedAssemblies();
      if (referencedAssemblies == null) return;
      foreach (var assemblyName in referencedAssemblies)
      {
         if (loadedAssemblies.Contains(assemblyName.Name)) continue;
         if(!(assemblyName.Name?.Contains(name)??false)) continue; 
         
         Assembly.Load(assemblyName);
         assemblies.Add(assemblyName.FullName);
         Debug.WriteLine($"Assembly chargé : {assemblyName.FullName}");
      }
      
   }

   public bool LoadDll(string name)
   {
      return LoadAbsolutePath(name);
      //var path = AppDomain.CurrentDomain.BaseDirectory + name +".dll";
      //return LoadAbsolutePath(path);
   }

   bool LoadAbsolutePath(string path)
   {
      //if (!File.Exists(path)) return false;

      if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == path)) return false;

      try
      {
         var assembly = Assembly.Load(path);

         foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
         {

            LoadDll(referencedAssembly.Name);
         }

         return true;
      }
      catch (FileNotFoundException)
      { }
      catch (BadImageFormatException)
      {
      }
      return false;
   }

   public void LoadModules()
   {

      var directory = AppDomain.CurrentDomain.BaseDirectory;
      if (directory != null) // on android 
      {
         var dlls = Directory.GetFiles(directory, "*.Module.dll");
         foreach (var path in dlls)
         {
            var filename = Path.GetFileNameWithoutExtension(path);
            LoadAbsolutePath(filename);
         }
      }

   }

}