using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class PropertyValidationContext<T>
    {
        internal readonly IEnumerable<PropertyRuleset> _rulesets;
        internal readonly PropertyRuleset<T> _currentRuleset;

        internal PropertyValidationContext(IEnumerable<PropertyRuleset> rulesets, Expression<Func<T>> propExpression)
        {
            _rulesets = rulesets;
            _currentRuleset = new PropertyRuleset<T>(propExpression);
        }

        internal PropertyValidationContext(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<T> currentRuleset)
        {
            _rulesets = rulesets;
            _currentRuleset = currentRuleset;
        }

        public PropertyPredicateContext<T> Where(Func<T, bool> predicate)
        {
            return new PropertyPredicateContext<T>(_rulesets, _currentRuleset, predicate);
        }

        public PropertyValidationContext<TNew> For<TNew>(Expression<Func<TNew>> propExpression)
        {
            return new PropertyValidationContext<TNew>(
                _rulesets.Concat(new [] {_currentRuleset}),
                propExpression
                );
        }

        public NumericPropValidationContext<int> For(Expression<Func<int>> propExpression)
        {
            return new NumericPropValidationContext<int>(
                _rulesets.Concat(new[] { _currentRuleset }),
                propExpression
                );
        }

        public ValidationRules Build()
        {
            var rules = new ValidationRules();

            var allRulesets = _rulesets.Concat(new[] {_currentRuleset});

            foreach (var ruleset in allRulesets)
            {
                ruleset.AddTo(rules);
            }

            return rules;
        }
    }
}