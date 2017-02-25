using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Assisticant.Validation
{
    public class StringPropertyValidationContext : PropertyValidationContext<string>
    {
        internal StringPropertyValidationContext(IEnumerable<PropertyRuleset> rulesets, Expression<Func<string>> propExpression) : base(rulesets, propExpression)
        {
        }

        internal StringPropertyValidationContext(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<string> currentRuleset) : base(rulesets, currentRuleset)
        {
        }

        public StringPropertyValidationContext Matches(string pattern)
        {
            var regex = new Regex(pattern);

            var name = _currentRuleset.PropExpr.GetPropertyName();

            var ruleset = _currentRuleset.AddRule(
                    v => v == null || regex.IsMatch((string)v),
                    () => $"{name} is not valid");

            return new StringPropertyValidationContext(_rulesets, ruleset);
        }
    }
}