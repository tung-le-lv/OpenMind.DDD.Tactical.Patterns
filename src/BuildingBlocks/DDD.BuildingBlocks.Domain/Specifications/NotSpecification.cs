using System.Linq.Expressions;

namespace BuildingBlocks.Domain.Specifications;

internal class NotSpecification<T>(Specification<T> specification) : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var negated = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }
}
