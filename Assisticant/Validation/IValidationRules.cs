using System.Collections.Generic;

namespace Assisticant.Validation
{
    public interface IValidationRules
    {
        event ValidationRules.ErrorsChangedDelegate ErrorsChanged;
        bool HasErrors { get; }
        IEnumerable<string> GetErrors(string propertyName);
    }
}