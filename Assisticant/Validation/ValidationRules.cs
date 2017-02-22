using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public sealed class ValidationRules : IDisposable, IValidationRules
    {
        public delegate void ErrorsChangedDelegate(string propertyName);

        public event ErrorsChangedDelegate ErrorsChanged;

        private readonly Dictionary<string, PropertyValidator> _validatorByPropertyName =
            new Dictionary<string, PropertyValidator>();

        internal PropertyValidator ValidatorForProperty<T>(Expression<Func<T>> property)
        {
            var body = (MemberExpression)property.Body;
            var propertyName = body.Member.Name;
            PropertyValidator propertyValidator;
            if (!_validatorByPropertyName.TryGetValue(propertyName, out propertyValidator))
            {
                var function = property.Compile();
                propertyValidator = new PropertyValidator(propertyName, () => function(), Notify);
                _validatorByPropertyName.Add(propertyName, propertyValidator);
            }
            return propertyValidator;
        }

        public PropertyValidationContext<T> For<T>(Expression<Func<T>> propExpression)
        {
            return new PropertyValidationContext<T>(this, propExpression);
        }

        public StringPropertyValidationContext For(Expression<Func<string>> propExpression)
        {
            return new StringPropertyValidationContext(this, propExpression);
        }

        public NumericPropValidationContext<int> For(Expression<Func<int>> propExpression)
        {
            return new NumericPropValidationContext<int>(this, propExpression);
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
