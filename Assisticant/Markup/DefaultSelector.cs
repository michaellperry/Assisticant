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
                return element.TryFindResource(new DataTemplateKey(proxy.Instance.GetType())) as DataTemplate;
            return null;
        }
    }
}
