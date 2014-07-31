using Assisticant.Metas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Descriptors
{
    public class ProxyTypeDescriptor : CustomTypeDescriptor
    {
        public readonly TypeMeta Meta;
        readonly ProxyPropertyDescriptor[] _properties;
        readonly PropertyDescriptorCollection _propertyCollection;
        readonly EventDescriptorCollection _events;
        public readonly Type ProxyType;

        public ProxyTypeDescriptor(Type type)
        {
            Meta = TypeMeta.Get(type);
            ProxyType = typeof(PlatformProxy<>).MakeGenericType(type);
            _properties = Meta.Members.Select(m => new ProxyPropertyDescriptor(this, m)).ToArray();
            _propertyCollection = new PropertyDescriptorCollection(_properties);
            _events = new EventDescriptorCollection(type.GetEvents().Select(e => new ProxyEventDescriptor(e)).ToArray());
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return _propertyCollection;
        }

        public override EventDescriptorCollection GetEvents()
        {
            return _events;
        }

        public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return _events;
        }

        public override string ToString()
        {
            return Meta.Type.Name;
        }
    }
}
