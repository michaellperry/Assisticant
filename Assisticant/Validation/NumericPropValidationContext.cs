using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class NumericPropValidationContextNew<T> : PropertyValidationContextNew<T>
        where T : IComparable<T>, IComparable
    {
        internal NumericPropValidationContextNew(
            IEnumerable<PropertyRuleset> rulesets,
            Expression<Func<T>> propExpression
            ) : base(rulesets, propExpression)
        {
        }

        internal NumericPropValidationContextNew(
            IEnumerable<PropertyRuleset> rulesets,
            PropertyRuleset<T> currentRuleset
            ) : base(rulesets, currentRuleset)
        {
        }

        public NumericPropValidationContextNew<T> GreaterThan(T lowerBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, lowerBound) > 0,
                () => $"{name} must be greater than {lowerBound}");

            return new NumericPropValidationContextNew<T>(_rulesets, ruleset);
        }

        public NumericPropValidationContextNew<T> GreaterThanOrEqualTo(T lowerBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, lowerBound) >= 0,
                () => $"{name} must be at least {lowerBound}");

            return new NumericPropValidationContextNew<T>(_rulesets, ruleset);
        }

        public NumericPropValidationContextNew<T> LessThan(T upperBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, upperBound) < 0,
                () => $"{name} must be less than {upperBound}");

            return new NumericPropValidationContextNew<T>(_rulesets, ruleset);
        }

        public NumericPropValidationContextNew<T> LessThanOrEqualTo(T upperBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, upperBound) <= 0,
                () => $"{name} must be no more than {upperBound}");

            return new NumericPropValidationContextNew<T>(_rulesets, ruleset);
        }

        private Comparer<T> Comparer => Comparer<T>.Default;
    }

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