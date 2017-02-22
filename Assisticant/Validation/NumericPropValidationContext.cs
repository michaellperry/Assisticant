using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class NumericPropValidationContext<T> : PropertyValidationContext<T>
        where T : IComparable<T>, IComparable
    {
        public NumericPropValidationContext(ValidationRules wrapped, Expression<Func<T>> propExpression) 
            : base(wrapped, propExpression)
        {
        }

        public ValidationRules GreaterThan(T lowerBound)
        {
            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator.AddRule(
                x => Comparer.Compare((T)x, lowerBound) > 0,
                () => $"{validator.PropertyName} must be greater than {lowerBound}");

            return _wrapped;
        }

        public ValidationRules GreaterThanOrEqualTo(T lowerBound)
        {
            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator.AddRule(
                x => Comparer.Compare((T)x, lowerBound) >= 0,
                () => $"{validator.PropertyName} must be at least {lowerBound}");

            return _wrapped;
        }

        public ValidationRules LessThan(T upperBound)
        {
            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator.AddRule(
                x => Comparer.Compare((T)x, upperBound) < 0,
                () => $"{validator.PropertyName} must be less than {upperBound}");

            return _wrapped;
        }

        public ValidationRules LessThanOrEqualTo(T upperBound)
        {
            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator.AddRule(
                x => Comparer.Compare((T)x, upperBound) <= 0,
                () => $"{validator.PropertyName} must be no more than {upperBound}");

            return _wrapped;
        }

        private Comparer<T> Comparer => Comparer<T>.Default;
    }
}