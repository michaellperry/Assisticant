using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public abstract class PropertyValidationContextBase<T>
    {
        internal readonly IEnumerable<PropertyRuleset> _rulesets;
        internal readonly PropertyRuleset<T> _currentRuleset;

        internal PropertyValidationContextBase(IEnumerable<PropertyRuleset> rulesets, Expression<Func<T>> propExpression)
        {
            _rulesets = rulesets;
            _currentRuleset = new PropertyRuleset<T>(propExpression);
        }

        internal PropertyValidationContextBase(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<T> currentRuleset)
        {
            _rulesets = rulesets;
            _currentRuleset = currentRuleset;
        }

        public string PropertyName => _currentRuleset.PropExpr.GetPropertyName();

        public PropertyPredicateContext<T> Where(Func<T, bool> predicate)
        {
            return new PropertyPredicateContext<T>(_rulesets, _currentRuleset, predicate);
        }

        public PropertyValidationContext<TNew> For<TNew>(Expression<Func<TNew>> propExpression)
        {
            return new PropertyValidationContext<TNew>(
                FinalizeRulesets(),
                propExpression
            );
        }

        internal OptionalMessagePropertyValidationContext<T> BeginPredicate(
            Func<T, bool> predicate,
            Func<string> messageFactory)
        {
            return new OptionalMessagePropertyValidationContext<T>(
                _rulesets,
                FinalizeRuleset(),
                predicate,
                messageFactory);
        }

        internal virtual IEnumerable<PropertyRuleset> FinalizeRulesets()
        {
            return _rulesets.Concat(new [] {FinalizeRuleset()});
        }

        internal virtual PropertyRuleset<T> FinalizeRuleset()
        {
            return _currentRuleset;
        }

        public ValidationRules Build()
        {
            var rules = new ValidationRules();

            var allRulesets = FinalizeRulesets();

            foreach (var ruleset in allRulesets)
            {
                ruleset.AddTo(rules);
            }

            return rules;
        }
    }
}