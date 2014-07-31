using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;

namespace Assisticant.XamlTypes
{
    public class ProxyXamlMetadataProvider : IXamlMetadataProvider
    {
        public IXamlType GetXamlType(string fullName)
        {
            return null;
        }

        public IXamlType GetXamlType(Type type)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(PlatformProxy<>))
                return ProxyXamlType.Get(type.GenericTypeArguments[0]);
            return null;
        }

        public XmlnsDefinition[] GetXmlnsDefinitions()
        {
            return null;
        }
    }
}
