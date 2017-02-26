using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class PropertyValidationContext<T>
    {
        internal readonly IEnumerable<PropertyRuleset> _rulesets;
        internal readonly Expression<Func<T>> _currentProperty;
        internal readonly IEnumerable<Tuple<Func<T, bool>, Func<string>>> _rules;
        internal readonly Func<T, bool> _currentPredicate;
        internal readonly Func<string> _currentMessageFactory;

        internal PropertyValidationContext(
            IEnumerable<PropertyRuleset> rulesets,
            Expression<Func<T>> currentProperty) :
            this (rulesets, currentProperty,
                new Tuple<Func<T, bool>, Func<string>>[0], null, null)
        {
        }

        internal PropertyValidationContext(
            IEnumerable<PropertyRuleset> rulesets,
            Expression<Func<T>> currentProperty,
            IEnumerable<Tuple<Func<T, bool>, Func<string>>> rules,
            Func<T, bool> currentPredicate,
            Func<string> currentMessageFactory)
        {
            _rulesets = rulesets;
            _currentProperty = currentProperty;
            _rules = rules;
            _currentPredicate = currentPredicate;
            _currentMessageFactory = currentMessageFactory;
        }

        public PropertyPredicateContext<T> Where(Func<T, bool> predicate)
        {
            return BeginPredicate(predicate);
        }

        public PropertyValidationContext<TNew> For<TNew>(Expression<Func<TNew>> propExpression)
        {
            var rulesets = EndProperty();

            return new PropertyValidationContext<TNew>(
                rulesets,
                propExpression);
        }

        public ValidationRules Build()
        {
            var rules = new ValidationRules();

            var allRulesets = EndProperty();

            foreach (var ruleset in allRulesets)
            {
                ruleset.AddTo(rules);
            }

            return rules;
        }

        internal string PropertyName => _currentProperty.GetPropertyName();

        internal PropertyPredicateContext<T> BeginPredicate(Func<T, bool> predicate, Func<string> messageFactory = null)
        {
            if (messageFactory == null)
            {
                var name = _currentProperty.GetPropertyName();

                messageFactory = () => $"{name} is not valid";
            }

            var rules = EndRule();
            return new PropertyPredicateContext<T>(_rulesets, _currentProperty, rules,
                predicate, messageFactory);
        }

        private IEnumerable<Tuple<Func<T, bool>, Func<string>>> EndRule()
        {
            if (_currentPredicate != null)
            {
                var rule = Tuple.Create(_currentPredicate, _currentMessageFactory);
                return _rules.Concat(new[] { rule });
            }
            else
            {
                return _rules;
            }
        }

        private IEnumerable<PropertyRuleset> EndProperty()
        {
            if (_currentProperty != null)
            {
                var rules = EndRule();
                var ruleset = new PropertyRuleset<T>(_currentProperty, rules);
                return _rulesets.Concat(new[] { ruleset });
            }
            else
            {
                return _rulesets;
            }
        }
    }
}