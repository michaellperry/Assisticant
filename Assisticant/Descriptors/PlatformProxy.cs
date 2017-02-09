using Assisticant.Metas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Descriptors
{
    public abstract partial class PlatformProxy : ViewProxy, IDataErrorInfo
    {
        public string Error
        {
            get
            {
                var errorInfo = Instance as IDataErrorInfo;
                return errorInfo != null ? errorInfo.Error : null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                var errorInfo = Instance as IDataErrorInfo;
                return errorInfo != null ? errorInfo[columnName] : null;
            }
        }

        protected PlatformProxy(object instance, ProxyTypeDescriptor descriptor)
            : base(instance, descriptor.Meta)
        {
            PlatformProxy_NotifyDataErrorInfo();
        }

        public abstract ProxyTypeDescriptor GetTypeDescriptor();

        public override ViewProxy WrapObject(object value)
        {
            if (value == null)
                return null;
            return (PlatformProxy)Activator.CreateInstance(typeof(PlatformProxy<>).MakeGenericType(value.GetType()), value);
        }

        partial void PlatformProxy_NotifyDataErrorInfo();
    }

    [TypeDescriptionProvider(typeof(ProxyDescriptionProvider))]
    public sealed class PlatformProxy<TViewModel> : PlatformProxy
    {
        public static readonly ProxyTypeDescriptor TypeDescriptor = new ProxyTypeDescriptor(typeof(TViewModel));

        public PlatformProxy(object instance)
            : base(instance, TypeDescriptor)
        {
        }

        public override ProxyTypeDescriptor GetTypeDescriptor()
        {
            return TypeDescriptor;
        }
    }
}
