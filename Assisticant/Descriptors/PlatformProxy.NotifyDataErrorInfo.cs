using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Descriptors
{
    public abstract partial class PlatformProxy : INotifyDataErrorInfo
    {
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => false;

        public IEnumerable GetErrors(string propertyName)
        {
            return Enumerable.Empty<string>();
        }
    }
}
