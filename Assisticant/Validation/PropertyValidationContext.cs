using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

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

    public class StringPropertyValidationContext : PropertyValidationContext<string>
    {
        internal StringPropertyValidationContext(ValidationRules wrapped, Expression<Func<string>> propExpression) : base(wrapped, propExpression)
        {
        }

        public ValidationRules Matches(string pattern)
        {
            var regex = new Regex(pattern);

            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator
                .AddRule(
                    v => v == null || regex.IsMatch((string)v),
                    () => $"{validator.PropertyName} is not valid");

            return _wrapped;
        }

        public ValidationRules IsRequired()
        {
            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator
                .AddRule(
                    v => !string.IsNullOrEmpty((string)v),
                    () => $"{validator.PropertyName} is required");

            return _wrapped;
        }
    }
}