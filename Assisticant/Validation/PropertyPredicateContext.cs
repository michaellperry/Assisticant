using System;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public sealed class PropertyPredicateContext<T>
    {
        private readonly ValidationRules _wrapped;
        private readonly Expression<Func<T>> _propExpression;
        private readonly Func<T, bool> _predicate;

        internal PropertyPredicateContext(ValidationRules wrapped, Expression<Func<T>> propExpression, Func<T, bool> predicate)
        {
            _wrapped = wrapped;
            _propExpression = propExpression;
            _predicate = predicate;
        }

        public ValidationRules WithMessage(string message)
        {
            _wrapped.ValidatorForProperty(_propExpression).AddRule(v => _predicate((T)v), () => message);

            return _wrapped;
        }
    }
}