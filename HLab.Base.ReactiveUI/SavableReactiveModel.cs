using System.Runtime.CompilerServices;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public abstract class SavableReactiveModel : ReactiveModel, ISavable
{
    /// <summary>
    /// Object has been saved
    /// </summary>
    public bool Saved
    {
        get => _saved;
        set => this.SetAndRaise(ref _saved, value);
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

    public virtual void Save()
    {
        Saved = true;
    }
}