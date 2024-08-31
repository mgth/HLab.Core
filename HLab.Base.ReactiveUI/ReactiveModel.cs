#nullable enable
using System.Runtime.CompilerServices;
using HLab.Base.Disposables;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public abstract class ReactiveModel : ReactiveObject, IDisposable, ISavable
{
    public DisposeHelper Disposer { get; } = new();

    /// <summary>
    /// Object has been saved
    /// </summary>
    public bool Saved
    {
        get => _saved;
        set => SetAndRaise(ref _saved, value);
    }
    bool _saved;


    /// <summary>
    /// Set properties values unsetting saved flag if changed
    /// </summary>
    protected bool SetUnsavedValue<TRet>(ref TRet backingField, TRet value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<TRet>.Default.Equals(backingField, value)) return false;

        this.RaisePropertyChanging(propertyName);
        backingField = value;
        Saved = false;
        this.RaisePropertyChanged(propertyName);
        return true;
    }

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