using System.Linq.Expressions;
using System.Reflection;
using ReactiveUI;

namespace HLab.Base.ReactiveUI;

public static class ReactiveModelExtension
{
    static readonly MemberInfo SavedProperty = typeof(ISavable).GetMember("Saved")[0];

    public static void UnsavedOn<T>( this T @this,params Expression<Func<T,ISavable>>[] predicates) where T : SavableReactiveModel
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
}