using System.Text.RegularExpressions;

namespace Assisticant.Validation
{
    public static class StringPropertyValidationContextExtensions
    {
        public static OptionalMessagePropertyValidationContext<string> Matches(
            this PropertyValidationContextBase<string> context,
            string pattern)
        {
            var regex = new Regex(pattern);

            return context.BeginPredicate(
                v => v == null || regex.IsMatch(v),
                () => $"{context._currentRuleset.PropExpr.GetPropertyName()} is not valid."
            );
        }
    }
}
