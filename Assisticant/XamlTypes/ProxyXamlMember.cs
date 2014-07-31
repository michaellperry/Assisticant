using Assisticant.Metas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;

namespace Assisticant.XamlTypes
{
    public class ProxyXamlMember : IXamlMember
    {
        readonly IXamlType _owner;
        readonly MemberMeta _meta;

        public bool IsAttachable
        {
            get { return false; }
        }

        public bool IsDependencyProperty
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return !_meta.CanWrite; }
        }

        public string Name
        {
            get { return _meta.Name; }
        }

        public IXamlType TargetType
        {
            get { return _owner; }
        }

        public IXamlType Type
        {
            get
            {
                if (!_meta.IsViewModel)
                    return PrimitiveXamlType.Get(_meta.MemberType);
                return ProxyXamlType.Get(_meta.MemberType);
            }
        }

        public ProxyXamlMember(IXamlType owner, MemberMeta meta)
        {
            _owner = owner;
            _meta = meta;
        }

        public object GetValue(object proxy)
        {
            return BindingInterceptor.Current.GetValue(GetSlot(proxy));
        }

        public void SetValue(object proxy, object value)
        {
            BindingInterceptor.Current.SetValue(GetSlot(proxy), value);
        }

        MemberSlot GetSlot(object proxy)
        {
            return ((ViewProxy)proxy).LookupSlot(_meta);
        }
    }
}
