using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public sealed class PropertyPredicateContext<T> : PropertyValidationContext<T>
    {
        internal PropertyPredicateContext(
            IEnumerable<PropertyRuleset> rulesets,
            Expression<Func<T>> currentProperty,
            IEnumerable<Tuple<Func<T, bool>, Func<string>>> rules,
            Func<T, bool> currentPredicate,
            Func<string> currentMessageFactory) :
            base(rulesets, currentProperty, rules, currentPredicate, currentMessageFactory)
        {
        }

        public PropertyValidationContext<T> WithMessage(Func<string> messageFactory)
        {
            return new PropertyValidationContext<T>(
                _rulesets, _currentProperty, _rules, _currentPredicate, messageFactory);
        }
    }
}