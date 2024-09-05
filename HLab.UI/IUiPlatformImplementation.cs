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
    public static IUiPlatformImplementation? Implementation { get; set; } = null;

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
}

public interface IUiPlatformImplementation
{
    IOpenFileDialog CreateOpenFileDialog();
    ISaveFileDialog CreateSaveFileDialog();

    IEnumerable GetLogicalChildren(object fe);
}