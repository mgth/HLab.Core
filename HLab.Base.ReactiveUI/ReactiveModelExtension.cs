using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using DynamicData;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public static class ReactiveModelExtension
{
    static readonly MemberInfo SavedProperty = typeof(ISavable).GetMember("Saved")[0];

    public static void UnsavedOn<T>( this T @this,params Expression<Func<T,ISavable>>[] predicates) where T : ISavable
    {
        foreach(var predicate in predicates)
        {
            if (predicate.Body is not MemberExpression m) continue;

            var exp = Expression.Lambda<Func<T,bool>>(
                Expression.MakeMemberAccess(m, SavedProperty),
                predicate.Parameters
            );

            @this.WhenAnyValue(exp)
                .Subscribe(e =>
                {
                    if (e) return;
                    @this.Saved = false;
                });
        }
    }
    
    public static bool SetAndRaise<TRet>(
        this ReactiveObject @this,
        ref TRet backingField,
        TRet newValue,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
        {
            return false;
        }

        @this.RaisePropertyChanging(propertyName);
        backingField = newValue;
        @this.RaisePropertyChanged(propertyName);
        return true;
    }
    public static bool SetAndRaise<TRet>(
        this ReactiveObject @this,
        ref TRet backingField,
        TRet newValue,
        Action<TRet> action,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
        {
            return false;
        }

        @this.RaisePropertyChanging(propertyName);
        backingField = newValue;
        action(newValue);
        @this.RaisePropertyChanged(propertyName);
        return true;
    }
    
    public static bool SetOneToMany<T, TClass>(
        this TClass @this, 
        ref T field, 
        T value, 
        Func<T, IList<TClass>> getCollection,
        [CallerMemberName] string? propertyName = null
        )
        where TClass : ReactiveObject
    {
        //if (property is not PropertyHolder<T> p) return false;
            
        //if (p.Parent is not TClass target) return false;
            
        var oldValue = field;

        if (!SetAndRaise(@this, ref field, value, propertyName)) return false;
                    
        if (oldValue is not null)
        {
            var collection = getCollection(oldValue);
            collection?.Remove(@this);
        }
                    
        if (value is not null)
        {
            var collection = getCollection(value);
            collection?.Add(@this);
        }
                    
        return true;
    }

    public static bool SetOneToMany<T, TClass>(
        this TClass @this, 
        ref T field, 
        T value, 
        Func<T, SourceList<TClass>> getCollection,
        [CallerMemberName] string? propertyName = null
        )
        where TClass : ReactiveObject
    {
        //if (property is not PropertyHolder<T> p) return false;
            
        //if (p.Parent is not TClass target) return false;
            
        var oldValue = field;

        if (!SetAndRaise(@this, ref field, value, propertyName)) return false;
                    
        if (oldValue is not null)
        {
            var collection = getCollection(oldValue);
            collection?.Remove(@this);
        }
                    
        if (value is not null)
        {
            var collection = getCollection(value);
            collection?.Add(@this);
        }
                    
        return true;
    }

}