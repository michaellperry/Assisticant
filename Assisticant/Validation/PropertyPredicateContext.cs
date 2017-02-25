using System;
using System.Collections.Generic;

namespace Assisticant.Validation
{
    public sealed class PropertyPredicateContext<T>
    {
        private readonly IEnumerable<PropertyRuleset> _rulesets;
        private readonly PropertyRuleset<T> _currentRuleset;
        private readonly Func<T, bool> _predicate;

        internal PropertyPredicateContext(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<T> currentRuleset, Func<T, bool> predicate)
        {
            _rulesets = rulesets;
            _predicate = predicate;
            _currentRuleset = currentRuleset;
        }

        public PropertyValidationContext<T> WithMessage(Func<string> messageFactory)
        {
            var ruleset = _currentRuleset.AddRule(_predicate, messageFactory);

            return new PropertyValidationContext<T>(_rulesets, ruleset);
        }
    }
}