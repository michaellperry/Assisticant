using System;
using System.Collections.Generic;
using System.Linq;

namespace Assisticant.Validation
{
    public sealed class OptionalMessagePropertyValidationContext<T> : PropertyValidationContextBase<T>
    {
        private readonly Func<T, bool> _predicate;
        private readonly Func<string> _messageFactory;

        internal OptionalMessagePropertyValidationContext(
            IEnumerable<PropertyRuleset> rulesets,
            PropertyRuleset<T> ruleset,
            Func<T, bool> predicate,
            Func<string> messageFactory  
        ) : base(rulesets, ruleset)
        {
            _predicate = predicate;
            _messageFactory = messageFactory;
        }

        public PropertyValidationContext<T> WithMessage(Func<string> messageFactory)
        {
            return new PropertyValidationContext<T>(
                _rulesets,
                FinalizeRuleset(messageFactory)
            );
        }

        internal override IEnumerable<PropertyRuleset> FinalizeRulesets()
        {
            return FinalizeRulesets(_messageFactory);
        }

        private IEnumerable<PropertyRuleset> FinalizeRulesets(Func<string> messageFactory)
        {
            var ruleset = FinalizeRuleset(messageFactory);

            return _rulesets.Concat(new[] {ruleset});
        }

        internal override PropertyRuleset<T> FinalizeRuleset()
        {
            return _currentRuleset.AddRule(_predicate, _messageFactory);
        }

        internal PropertyRuleset<T> FinalizeRuleset(Func<string> messageFactory)
        {
            return _currentRuleset.AddRule(_predicate, messageFactory);
        }
    }
}