using Assisticant.Collections;
using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assisticant.Validation
{
    public class PropertyValidator : IDisposable
    {
        private readonly ObservableList<Func<object, string>> _rules = new ObservableList<Func<object, string>>();
        private readonly Computed<List<string>> _validationErrors;
        private readonly ComputedSubscription _subscription;

        public string PropertyName { get; }

        public PropertyValidator(string propertyName, Func<object> function, Action<string> notify)
        {
            PropertyName = propertyName;
            _validationErrors = new Computed<List<string>>(() =>
            {
                var value = function();
                return _rules.Select(r => r(value)).Where(e => e != null).ToList();
            });
            _subscription = _validationErrors.Subscribe((errors, priorErrors) =>
            {
                if (priorErrors == null)
                {
                    if (errors != null && errors.Any())
                    {
                        notify(propertyName);
                    }
                }
                else
                {
                    if (errors == null || !priorErrors.SequenceEqual(errors))
                    {
                        notify(propertyName);
                    }
                }
            });
        }

        public void Dispose()
        {
            _subscription.Unsubscribe();
        }

        public void AddRule(Func<object, string> rule)
        {
            _rules.Add(rule);
        }

        public void AddRule(Func<object, bool> predicate, Func<string> errGenerator)
        {
            _rules.Add(v => predicate(v) ? null : errGenerator());
        }

        public IEnumerable<string> ValidationErrors => _validationErrors.Value;
    }
}
