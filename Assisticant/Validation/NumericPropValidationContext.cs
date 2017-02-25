using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class NumericPropValidationContext<T> : PropertyValidationContext<T>
        where T : IComparable<T>, IComparable
    {
        internal NumericPropValidationContext(
            IEnumerable<PropertyRuleset> rulesets,
            Expression<Func<T>> propExpression
            ) : base(rulesets, propExpression)
        {
        }

        internal NumericPropValidationContext(
            IEnumerable<PropertyRuleset> rulesets,
            PropertyRuleset<T> currentRuleset
            ) : base(rulesets, currentRuleset)
        {
        }

        public NumericPropValidationContext<T> GreaterThan(T lowerBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, lowerBound) > 0,
                () => $"{name} must be greater than {lowerBound}");

            return new NumericPropValidationContext<T>(_rulesets, ruleset);
        }

        public NumericPropValidationContext<T> GreaterThanOrEqualTo(T lowerBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, lowerBound) >= 0,
                () => $"{name} must be at least {lowerBound}");

            return new NumericPropValidationContext<T>(_rulesets, ruleset);
        }

        public NumericPropValidationContext<T> LessThan(T upperBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, upperBound) < 0,
                () => $"{name} must be less than {upperBound}");

            return new NumericPropValidationContext<T>(_rulesets, ruleset);
        }

        public NumericPropValidationContext<T> LessThanOrEqualTo(T upperBound)
        {
            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                x => Comparer.Compare((T)x, upperBound) <= 0,
                () => $"{name} must be no more than {upperBound}");

            return new NumericPropValidationContext<T>(_rulesets, ruleset);
        }

        private Comparer<T> Comparer => Comparer<T>.Default;
    }
}