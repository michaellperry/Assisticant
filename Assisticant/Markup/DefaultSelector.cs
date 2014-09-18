using Assisticant.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Assisticant.Markup
{
    public class DefaultSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var proxy = item as PlatformProxy;
            var element = container as FrameworkElement;
            if (proxy != null && element != null)
            {
                for (var type = proxy.Instance.GetType(); type != null && type != typeof(object); type = type.BaseType)
                {
                    var template = element.TryFindResource(new DataTemplateKey(type)) as DataTemplate;
                    if (template != null)
                        return template;
                }
            }
            return null;
        }
    }
}
