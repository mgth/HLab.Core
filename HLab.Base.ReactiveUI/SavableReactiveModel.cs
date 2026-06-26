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

    // NOTE: intentionally NO parameterless Save() here.
    // Persistence is provided by extension methods (e.g. PersistencyExtensions.Save(this MonitorsLayout)),
    // which both write storage and reset the child Saved flags. A parameterless instance Save() on this
    // base would shadow those extensions at every `x.Save()` call site (instance methods win over
    // extension methods in overload resolution), silently turning persistence into a no-op.
}