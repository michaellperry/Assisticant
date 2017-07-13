namespace Assisticant.Validation
{
    public static class ObjectPropertyValidationContextExtensions
    {
        public static OptionalMessagePropertyValidationContext<T> NotNull<T>(
            this PropertyValidationContext<T> context)
            where T : class
        {
            return context.BeginPredicate(
                v => v != null,
                () => $"{context._currentRuleset.PropExpr.GetPropertyName()} must not be null."
            );
        }
    }
}
