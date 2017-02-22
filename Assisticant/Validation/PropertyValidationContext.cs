using System;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class PropertyValidationContext<T>
    {
        protected readonly ValidationRules _wrapped;
        protected readonly Expression<Func<T>> _propExpression;

        internal PropertyValidationContext(ValidationRules wrapped, Expression<Func<T>> propExpression)
        {
            _wrapped = wrapped;
            _propExpression = propExpression;
        }

        public PropertyPredicateContext<T> Where(Func<T, bool> predicate)
        {
            return new PropertyPredicateContext<T>(_wrapped, _propExpression, predicate);
        }
    }
}