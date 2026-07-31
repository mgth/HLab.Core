using System.Runtime.CompilerServices;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public abstract class SavableReactiveModel : ReactiveModel, ISavable
{
    /// <summary>
    /// How many times anything in this process has been marked unsaved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Saved"/> is a state, and a state only transitions once: the second edit
    /// after a save notifies nobody, which is exactly why persistence has to walk the whole
    /// tree marking every child saved again — otherwise the reactive chains built on those
    /// transitions go quiet for good. A consumer asking "has anything moved since I last
    /// looked?" therefore has no state to watch, however carefully it subscribes.
    /// </para>
    /// <para>
    /// This is that notification, and it lives on the flag itself so that no model can be
    /// added, nested or reshaped without it. Deliberately coarse and process-wide: a
    /// consumer that re-reads a whole object graph does not care which node moved, and a
    /// per-node signal threaded up through every nested model is precisely the plumbing
    /// that gets forgotten the next time a field is added. Monotonic, so comparing two
    /// readings is the entire protocol.
    /// </para>
    /// </remarks>
    public static long Revision => Interlocked.Read(ref _revision);
    static long _revision;

    /// <summary>
    /// Object has been saved
    /// </summary>
    public bool Saved
    {
        get => _saved;
        set
        {
            // Every request to unset counts, including one that finds the flag already
            // unset — that is the case the transition cannot carry.
            if (!value) Interlocked.Increment(ref _revision);
            this.SetAndRaise(ref _saved, value);
        }
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
    // Persistence is provided externally (LittleBigMouse.Plugins.Persistence.LayoutPersistence),
    // which both writes storage and resets the child Saved flags. A parameterless instance Save() on
    // this base would shadow any extension method at a `x.Save()` call site (instance methods win over
    // extension methods in overload resolution), silently turning persistence into a no-op.
}