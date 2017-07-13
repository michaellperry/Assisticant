using System.Text.RegularExpressions;

namespace Assisticant.Validation
{
    public static class StringPropertyValidationContextExtensions
    {
        public static OptionalMessagePropertyValidationContext<string> Required(
            this PropertyValidationContext<string> context)
        {
            return context.BeginPredicate(
                v => !string.IsNullOrWhiteSpace(v),
                () => $"{context._currentRuleset.PropExpr.GetPropertyName()} is required."
            );
        }

        public static OptionalMessagePropertyValidationContext<string> MaxLength(
            this PropertyValidationContext<string> context,
            int bound)
        {
            return context.BeginPredicate(
                v => v == null || v.Length <= bound,
                () => $"{context._currentRuleset.PropExpr.GetPropertyName()} may be at most {bound} characters long."
            );
        }

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
