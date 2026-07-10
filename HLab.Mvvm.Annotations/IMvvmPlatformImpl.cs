using System;

namespace HLab.Mvvm.Annotations;

public interface IMvvmPlatformImpl
{
    /// <summary>
    /// Provide a fallback view when no view could be resolved.
    /// </summary>
    IView GetNotFoundView(Type getType, Type viewMode, Type viewClass);

    /// <summary>
    /// Prepares the specified view for use by setting its view class and view mode.
    /// Dispatches to the UI thread when called from elsewhere.
    /// </summary>
    void PrepareView(IView view);

    void Register(IMvvmService mvvm);

    /// <summary>
    /// Registers the specified type with the MVVM platform implementation.
    /// </summary>
    /// <param name="type">The type to be registered. This type should not be an interface.</param>
    void Register(Type type);

    /// <summary>
    /// Called when a view is activated
    /// </summary>
    object Activate(IView obj);

    object Deactivate(IView obj);

    IWindow ViewAsWindow(IView? view);
    IWindow ViewAsWindow<T>(IView? view) where T: IWindow, new();
}
