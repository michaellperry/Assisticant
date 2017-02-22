using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Assisticant.Validation
{
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