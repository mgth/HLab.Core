#nullable enable
using System.Runtime.CompilerServices;
using HLab.Base.Disposables;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public abstract class ReactiveModel : ReactiveObject, IDisposable
{
    public DisposeHelper Disposer { get; } = new();

    public bool SetAndRaise<TRet>(
        ref TRet backingField,
        TRet newValue,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
        {
            return false;
        }

        this.RaisePropertyChanging(propertyName);
        backingField = newValue;
        this.RaisePropertyChanged(propertyName);
        return true;
    }

    public virtual void OnDispose()
    {
    }

    public void Dispose()
    {
        OnDispose();
        Disposer.Dispose();
    }
}