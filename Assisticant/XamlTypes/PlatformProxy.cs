using Assisticant.Metas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.XamlTypes
{
    public class PlatformProxy : ViewProxy, INotifyDataErrorInfo
    {
        public bool HasErrors
        {
            get
            {
                var errors = Instance as INotifyDataErrorInfo;
                if (errors != null)
                    return errors.HasErrors;
                else
                    return false;
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged { add { } remove { } }

        protected PlatformProxy(object instance, TypeMeta type)
            : base(instance, type)
        {
        }

        public IEnumerable GetErrors(string propertyName)
        {
            var errors = Instance as INotifyDataErrorInfo;
            if (errors != null)
                return errors.GetErrors(propertyName);
            else
                return Enumerable.Empty<object>();
        }

        public override ViewProxy WrapObject(object value)
        {
            if (value == null)
                return null;
            return (PlatformProxy)Activator.CreateInstance(typeof(PlatformProxy<>).MakeGenericType(value.GetType()), value);
        }
    }

    public class PlatformProxy<TViewModel> : PlatformProxy
    {
        public PlatformProxy(object instance)
            : base(instance, TypeMeta.Get(typeof(TViewModel)))
        {
        }
    }
}
