using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Assisticant.Validation
{
    public class StringPropertyValidationContextNew : PropertyValidationContextNew<string>
    {
        internal StringPropertyValidationContextNew(IEnumerable<PropertyRuleset> rulesets, Expression<Func<string>> propExpression) : base(rulesets, propExpression)
        {
        }

        internal StringPropertyValidationContextNew(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<string> currentRuleset) : base(rulesets, currentRuleset)
        {
        }

        public StringPropertyValidationContextNew Matches(string pattern)
        {
            var regex = new Regex(pattern);

            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                    v => v == null || regex.IsMatch((string)v),
                    () => $"{name} is not valid");

            return new StringPropertyValidationContextNew(_rulesets, ruleset);
        }
    }

    public class StringPropertyValidationContext : PropertyValidationContext<string>
    {
        internal StringPropertyValidationContext(ValidationRules wrapped, Expression<Func<string>> propExpression) : base(wrapped, propExpression)
        {
        }

        public ValidationRules Matches(string pattern)
        {
            var regex = new Regex(pattern);

            var validator = _wrapped.ValidatorForProperty(_propExpression);

            validator
                .AddRule(
                    v => v == null || regex.IsMatch((string)v),
                    () => $"{validator.PropertyName} is not valid");

            return _wrapped;
        }
    }
}