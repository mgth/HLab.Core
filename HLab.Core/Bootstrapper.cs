using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HLab.Base;
using HLab.Core.Annotations;

namespace HLab.Core;

/// <summary>
/// 
/// </summary>
public class Bootstrapper(Func<IEnumerable<Bootloader>> getBootloaders) : IBootstrapper
{

   readonly ConcurrentQueue<Bootloader> _queue = new();
   readonly HashSet<Bootloader> _completed = [];
   
   public async Task BootAsync()
   {
      var bl = getBootloaders();

      var bootLoaders = Sort(bl);

      foreach (var bootloader in bootLoaders)
      {
         bootloader.Bootstrapper = this;
         _queue.Enqueue(bootloader);
      }

      while (_queue.TryDequeue(out var bootloader))
      {
         bootloader.State = await bootloader.LoadAsync();
         switch (bootloader.State)
         {
            case BootState.Completed:
               _completed.Add(bootloader);
               break;
               
            case BootState.Failed:
               throw new Exception($"Bootloader {bootloader.Name} failed");
               
            case BootState.Requeue:
               if (bootloader.LastQueueSize == _queue.Count)
               {
                  await bootloader.LoadAsync();
                  //throw new Exception($"Bootloader {bootloader.Name} is deadlocked");
               }
               
               bootloader.LastQueueSize = _queue.Count;
               
               _queue.Enqueue(bootloader);
               
               break;
               
            case BootState.Running:
            case BootState.Cancel:
            case BootState.Waiting:
            default:
               throw new ArgumentOutOfRangeException();
         }

      }
   }

   static IEnumerable<T> Sort<T>(IEnumerable<T> src)
   {
      var result = new List<T>();
      foreach (var boot in src)
      {
         var bootAssemblyName = boot.GetType().Assembly.GetName().Name;
         for (var i = 0; i < result.Count; i++)
         {
            var a = result[i].GetType().Assembly;
            if (a.References(bootAssemblyName))
            {
               result.Insert(i, boot);
               goto inserted;
            }
         }
         result.Add(boot);
      inserted:;
      }

      return result;
   }

   //private readonly Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();

   public bool Contains(params string[] names)
       => names.Any(name => _queue.Any(e => e.Name.EndsWith(name)));
}