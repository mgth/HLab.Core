using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HLab.Core.Annotations;
// TODO : Progress for bootloader

public interface IBootloader
{
   
}

public abstract class Bootloader : ReactiveObject, IBootloader
{
   protected Bootloader()
   {
      Name = GetType().FullName;
   }
   
   public int LastQueueSize {get ; set; }
   
   public IBootstrapper? Bootstrapper {get ;  set; }
   public string Name {get ; } 

   public BootState State { get;  set; } = BootState.Waiting;
   
   public virtual Task<BootState> LoadAsync()
   {
      return Task.Run(Load);
   }
   
   protected virtual BootState Load()
   {  
      return BootState.Completed;
   }
   

   public override string ToString() => Name;
   
   protected bool WaitingForBootloader(params string[] name) => Bootstrapper.Contains(name);

   protected bool WaitingForServices(params IService[] service) => service.Any(s => s.ServiceState == ServiceState.NotConfigured);

   protected bool WaitingForBootloader<T>() => WaitingForBootloader(typeof(T).Name);
}
/*

public class MyBootloader : IBootloader
{
    public MyBootloader() // <- Inject here
    {
    }

    public void Load(IBootContext b)
    {
    }

}

*/