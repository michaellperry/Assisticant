using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class PropertyValidationContextNew<T>
    {
        internal readonly IEnumerable<PropertyRuleset> _rulesets;
        internal readonly PropertyRuleset<T> _currentRuleset;

        internal PropertyValidationContextNew(IEnumerable<PropertyRuleset> rulesets, Expression<Func<T>> propExpression)
        {
            _rulesets = rulesets;
            _currentRuleset = new PropertyRuleset<T>(propExpression);
        }

        internal PropertyValidationContextNew(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<T> currentRuleset)
        {
            _rulesets = rulesets;
            _currentRuleset = currentRuleset;
        }

        public PropertyPredicateContextNew<T> Where(Func<T, bool> predicate)
        {
            return new PropertyPredicateContextNew<T>(_rulesets, _currentRuleset, predicate);
        }

        public PropertyValidationContextNew<TNew> For<TNew>(Expression<Func<TNew>> propExpression)
        {
            return new PropertyValidationContextNew<TNew>(
                _rulesets.Concat(new [] {_currentRuleset}),
                propExpression
                );
        }

        public StringPropertyValidationContextNew For(Expression<Func<string>> propExpression)
        {
            return new StringPropertyValidationContextNew(
                _rulesets.Concat(new[] { _currentRuleset }),
                propExpression
                );
        }

        public NumericPropValidationContextNew<int> For(Expression<Func<int>> propExpression)
        {
            return new NumericPropValidationContextNew<int>(
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

    public sealed class PropertyPredicateContextNew<T>
    {
        private readonly IEnumerable<PropertyRuleset> _rulesets;
        private readonly PropertyRuleset<T> _currentRuleset;
        private readonly Func<T, bool> _predicate;

        internal PropertyPredicateContextNew(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<T> currentRuleset, Func<T, bool> predicate)
        {
            _rulesets = rulesets;
            _predicate = predicate;
            _currentRuleset = currentRuleset;
        }

        public PropertyValidationContextNew<T> WithMessage(Func<string> messageFactory)
        {
            var ruleset = _currentRuleset.AddRule(_predicate, messageFactory);

            return new PropertyValidationContextNew<T>(_rulesets, ruleset);
        }
    }

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