using System;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public static class IntValidationExtension
    {
        public static ValidationRules ForInt(this ValidationRules validator, Expression<Func<int>> property,
            Func<IntPropertyValidationRule, IntPropertyValidationRule> rule)
        {
            var propertyValidator = validator.ValidatorForProperty(property);
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

        public IntPropertyValidationRule GreaterThan(int lowerBound)
        {
            _propertyValidator.AddRule(v => ((int)v) > lowerBound
                ? null
                : $"{_propertyValidator.PropertyName} must be greater than {lowerBound}");
            return this;
        }

        public IntPropertyValidationRule GreaterThanOrEqualTo(int lowerBound)
        {
            _propertyValidator.AddRule(v => ((int)v) >= lowerBound
                ? null
                : $"{_propertyValidator.PropertyName} must be at least {lowerBound}");
            return this;
        }

        public IntPropertyValidationRule LessThan(int upperBound)
        {
            _propertyValidator.AddRule(v => ((int)v) < upperBound
                ? null
                : $"{_propertyValidator.PropertyName} must be less than {upperBound}");
            return this;
        }

        public IntPropertyValidationRule LessThanOrEqualTo(int upperBound)
        {
            _propertyValidator.AddRule(v => ((int)v) <= upperBound
                ? null
                : $"{_propertyValidator.PropertyName} must be no more than {upperBound}");
            return this;
        }
    }
}
