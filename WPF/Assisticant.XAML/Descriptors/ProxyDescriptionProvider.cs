using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.XAML.Descriptors
{
    class ProxyDescriptionProvider : TypeDescriptionProvider
    {
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var objectInstance = instance as ViewProxy;
            if (objectInstance != null)
                return objectInstance.GetTypeDescriptor();

            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ViewProxy<>))
            {
                var field = objectType.GetField("TypeDescriptor", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                return (ICustomTypeDescriptor)field.GetValue(null);
            }

            return new ProxyTypeDescriptor(objectType);
        }
    }
}
