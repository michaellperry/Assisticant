using System;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    internal static class ExpressionExtensions
    {
        public static string GetPropertyName<T>(this Expression<Func<T>> expr)
        {
            var body = (MemberExpression)expr.Body;
            var propertyName = body.Member.Name;

            return propertyName;
        }
    }
}
