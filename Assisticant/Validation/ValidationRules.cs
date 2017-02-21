using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public class ValidationRules : IDisposable
    {
        public delegate void ErrorsChangedDelegate(string propertyName);

        public event ErrorsChangedDelegate ErrorsChanged;

        private Dictionary<string, PropertyValidator> _validatorByPropertyName =
            new Dictionary<string, PropertyValidator>();

        public PropertyValidator ValidatorForProperty<T>(Expression<Func<T>> property)
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

        public ValidationRules For<T>(Expression<Func<T>> property, Func<T, bool> predicate, Func<string> message)
        {
            var validator = ValidatorForProperty(property);
            validator.AddRule(v => predicate((T)v) ? null : message());
            return this;
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
