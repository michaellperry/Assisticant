using Assisticant.Metas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Descriptors
{
    public class ProxyPropertyDescriptor : PropertyDescriptor
    {
        readonly ProxyTypeDescriptor _owner;
        readonly MemberMeta _meta;
        readonly Type _exposedType;

        public override Type ComponentType
        {
            get { return _owner.ProxyType; }
        }

        public override Type PropertyType
        {
            get { return _exposedType; }
        }

        public override bool IsReadOnly
        {
            get { return !_meta.CanWrite; }
        }

        public ProxyPropertyDescriptor(ProxyTypeDescriptor owner, MemberMeta meta)
            : base(meta.Name, null)
        {
            _owner = owner;
            _meta = meta;
            if (!meta.IsViewModelType)
                _exposedType = meta.MemberType;
            else if (typeof(IEnumerable).IsAssignableFrom(meta.MemberType))
                _exposedType = typeof(IEnumerable);
            else
                _exposedType = typeof(object);
        }

        public override object GetValue(object proxy)
        {
            return BindingInterceptor.Current.GetValue(GetSlot(proxy));
        }

        public override void SetValue(object proxy, object value)
        {
            BindingInterceptor.Current.SetValue(GetSlot(proxy), value);
        }

        public override bool CanResetValue(object proxy)
        {
            return false;
        }

        public override void ResetValue(object proxy)
        {
        }

        public override bool ShouldSerializeValue(object proxy)
        {
            return false;
        }

        MemberSlot GetSlot(object proxy)
        {
            return ((ViewProxy)proxy).LookupSlot(_meta);
        }

        public override string ToString()
        {
            return _meta.ToString();
        }
    }
}
