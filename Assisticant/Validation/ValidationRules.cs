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

        public PropertyValidator AddPropertyValidator<T>(Expression<Func<T>> property)
        {
            var body = (MemberExpression)property.Body;
            var propertyName = body.Member.Name;
            var function = property.Compile();
            var propertyValidator = new PropertyValidator(propertyName, () => function, Notify);
            _validatorByPropertyName.Add(propertyName, propertyValidator);
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
