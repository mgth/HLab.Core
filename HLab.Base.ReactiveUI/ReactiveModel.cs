#nullable enable
using System.Diagnostics;
using System.Runtime.CompilerServices;
using HLab.Base.Disposables;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public abstract class ReactiveModel : ReactiveObject, IDisposable
{
    public DisposeHelper Disposer { get; } = new();

    public virtual void OnDispose()
    {
    }

    public void Dispose()
    {
        OnDispose();
        Disposer.Dispose();
    }
}