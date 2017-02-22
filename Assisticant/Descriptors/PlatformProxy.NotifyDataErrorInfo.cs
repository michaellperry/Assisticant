using Assisticant.Validation;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace Assisticant.Descriptors
{
    public abstract partial class PlatformProxy : INotifyDataErrorInfo
    {
        private IValidationRules _validator;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        partial void PlatformProxy_NotifyDataErrorInfo()
        {
            var validation = Instance as IValidation;
            _validator = validation?.Rules;
            if (_validator != null)
            {
                _validator.ErrorsChanged += Validator_ErrorsChanged;
            }
        }

        public bool HasErrors => _validator?.HasErrors ?? false;

        public IEnumerable GetErrors(string propertyName)
        {
            return _validator?.GetErrors(propertyName) ?? Enumerable.Empty<string>();
        }

        private void Validator_ErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
