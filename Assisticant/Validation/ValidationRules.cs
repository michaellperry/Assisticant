using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public static class Validator
    {
        public static PropertyValidationContext<T> For<T>(Expression<Func<T>> propExpr)
        {
            return new PropertyValidationContext<T>(new PropertyRuleset[0], propExpr);
        }

        public static PropertyValidationContext<int> For(Expression<Func<int>> propExpr)
        {
            return new PropertyValidationContext<int>(new PropertyRuleset[0], propExpr);
        }
    }

    public sealed class ValidationRules : IDisposable, IValidationRules
    {
        public delegate void ErrorsChangedDelegate(string propertyName);

        public event ErrorsChangedDelegate ErrorsChanged;

        private readonly Dictionary<string, PropertyValidator> _validatorByPropertyName =
            new Dictionary<string, PropertyValidator>();

        internal ValidationRules()
        {
            
        }

        internal PropertyValidator ValidatorForProperty<T>(Expression<Func<T>> property)
        {
            var propertyName = property.GetPropertyName();
            PropertyValidator propertyValidator;
            if (!_validatorByPropertyName.TryGetValue(propertyName, out propertyValidator))
            {
                var function = property.Compile();
                propertyValidator = new PropertyValidator(propertyName, () => function(), Notify);
                _validatorByPropertyName.Add(propertyName, propertyValidator);
            }
            return propertyValidator;
        }

        private void Notify(string propertyName)
        {
            ErrorsChanged?.Invoke(propertyName);
        }

        public void Dispose()
        {
            foreach (var propertyValidator in _validatorByPropertyName.Values)
                propertyValidator.Dispose();
        }

        public bool HasErrors => _validatorByPropertyName.Values
            .Any(p => p.ValidationErrors.Any());

        public IEnumerable<string> GetErrors(string propertyName)
        {
            PropertyValidator propertyValidator;
            if (_validatorByPropertyName.TryGetValue(propertyName, out propertyValidator))
                return propertyValidator.ValidationErrors;
            else
                return Enumerable.Empty<string>();
        }
    }
}
