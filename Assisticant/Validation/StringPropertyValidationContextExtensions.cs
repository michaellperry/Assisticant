using System.Text.RegularExpressions;

namespace Assisticant.Validation
{
    public static class StringPropertyValidationContextExtensions
    {
        public static PropertyPredicateContext<string> Matches(this PropertyValidationContext<string> context, string pattern)
        {
            var regex = new Regex(pattern);

            return new PropertyPredicateContext<string>(
                context._rulesets,
                context._currentRuleset,
                v => v == null || regex.IsMatch(v));
        }
    }
}
