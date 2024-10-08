﻿using System.Collections;

namespace HLab.UI;

public interface IFileDialog
{
    void Reset();
    string? SuggestedFileName { get; set; }
    string FileName { get; set; }
    string DefaultExt { get; set; }
    string Filter { get; set; }

}
public interface IOpenFileDialog : IFileDialog
{
    bool AllowMultiple { get; set; }

    Stream OpenFile();
}
public interface ISaveFileDialog : IFileDialog
{
}

public static class UiPlatform
{
    //public static IUiPlatformImplementation? Implementation { get; set; } = null;
    
    public static void Configure(IUiPlatformImplementation implementation)
    {
        Implementation = implementation;
    }

    static IUiPlatformImplementation? _implementation;
    static IUiPlatformImplementation Implementation
    {
        get => _implementation ?? throw new InvalidOperationException("UiPlatform.Implementation is not set");
        set => _implementation = value;
    }


    public static IEnumerable<TChild> FindLogicalChildren<TParent,TChild>(this TParent fe)
    {
        if (fe == null) yield break;
        foreach (var child in Implementation.GetLogicalChildren(fe))
        {
            if (child is TChild c)
            {
                yield return c;
            }

            if (child is not TParent e) continue;
            foreach (var childOfChild in FindLogicalChildren<TParent,TChild>(e))
            {
                yield return childOfChild;
            }
        }
    }
    
    public static Task InvokeOnUiThreadAsync(Action callback) => Implementation.InvokeOnUiThreadAsync(callback);
    public static Task InvokeOnUiThreadAsync(Func<Task> callback) => Implementation.InvokeOnUiThreadAsync(callback);
    public static void VerifyAccess() => Implementation.VerifyAccess();

    public static IGuiTimer CreateGuiTimer() => Implementation.CreateGuiTimer();
    
    public static string? GetClipboardText() => Implementation.GetClipboardText();
    public static void SetClipboardText(string text) => Implementation.SetClipboardText(text);

    public static void Quit() => Implementation.Quit();
}

public interface IUiPlatformImplementation
{
    IOpenFileDialog CreateOpenFileDialog();
    ISaveFileDialog CreateSaveFileDialog();

    IEnumerable GetLogicalChildren(object fe);

    Task InvokeOnUiThreadAsync(Action callback);
    Task InvokeOnUiThreadAsync(Func<Task> callback);

    void VerifyAccess();
    
    IGuiTimer CreateGuiTimer();
    
    string? GetClipboardText();
    void SetClipboardText(string text);

    void Quit();
}