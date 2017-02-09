using Assisticant.Collections;
using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assisticant.Validation
{
    public class PropertyValidator : IDisposable
    {
        private ObservableList<Func<object, string>> _rules = new ObservableList<Func<object, string>>();
        private Computed<List<string>> _validationErrors;
        private ComputedSubscription _subscription;

        public string PropertyName { get; }

        public PropertyValidator(string propertyName, Func<object> function, Action<string> notify)
        {
            PropertyName = propertyName;
            _validationErrors = new Computed<List<string>>(() =>
            {
                var value = function();
                return _rules.Select(r => r(value)).Where(e => e != null).ToList();
            });
            _subscription = _validationErrors.Subscribe(errors => notify(propertyName));
        }

        public void Dispose()
        {
            _subscription.Unsubscribe();
        }

        public void AddRule(Func<object, string> rule)
        {
            _rules.Add(rule);
        }

        public IEnumerable<string> ValidationErrors => _validationErrors.Value;
    }
}
