using System;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public static class IntValidationExtension
    {
        public static ValidationRules For(this ValidationRules validator, Expression<Func<int>> property,
            Func<IntPropertyValidationRule, IntPropertyValidationRule> rule)
        {
            var propertyValidator = validator.AddPropertyValidator(property);
            rule(new IntPropertyValidationRule(propertyValidator));
            return validator;
        }
    }

    public class IntPropertyValidationRule
    {
        private PropertyValidator _propertyValidator;

        public IntPropertyValidationRule(PropertyValidator propertyValidator)
        {
            _propertyValidator = propertyValidator;
        }

        public IntPropertyValidationRule GreaterThanOrEqualTo(int lowerBound)
        {
            _propertyValidator.AddRule(v => ((int)v) < lowerBound
                ? $"{_propertyValidator.PropertyName} must be greater than or equal to {lowerBound}"
                : null);
            return this;
        }

        public IntPropertyValidationRule LessThan(int upperBound)
        {
            _propertyValidator.AddRule(v => ((int)v) >= upperBound
                ? $"{_propertyValidator.PropertyName} must be less than {upperBound}"
                : null);
            return this;
        }
    }
}
