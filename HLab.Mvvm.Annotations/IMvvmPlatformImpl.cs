using System;
using System.Threading;
using System.Threading.Tasks;

namespace HLab.Mvvm.Annotations;

public interface IMvvmPlatformImpl
{
    /// <summary>
    /// Provide a default view 
    /// </summary>
    /// <param name="getType"></param>
    /// <param name="viewMode"></param>
    /// <param name="viewClass"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IView> GetNotFoundViewAsync(Type getType, Type viewMode, Type viewClass, CancellationToken token = default);

    /// <summary>
    /// Prepares the specified view for use by setting its view class and view mode.
    /// </summary>
    /// <param name="view">The view to be prepared.</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PrepareViewAsync(IView view, CancellationToken token = default);

    /// <summary>
    /// 
    /// </summary>
    void Register(IMvvmService mvvm);

    /// <summary>
    /// Registers the specified type with the MVVM platform implementation.
    /// </summary>
    /// <param name="type">The type to be registered. This type should not be an interface.</param>
    /// <remarks>
    /// This method is used to associate a type with the platform's MVVM infrastructure, enabling
    /// the platform to recognize and handle the type appropriately during runtime.
    /// </remarks>
    void Register(Type type);


    /// <summary>
    /// Called when a view is activated
    /// </summary>
    /// <param name="obj"></param>
    object Activate(IView obj);

    object Deactivate(IView obj);

    IWindow ViewAsWindow(IView? view); 
    IWindow ViewAsWindow<T>(IView? view) where T: IWindow, new(); 

}