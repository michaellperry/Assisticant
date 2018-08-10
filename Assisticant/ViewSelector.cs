/**********************************************************************
 * 
 * Assisticant
 * Copyright 2014 Michael L Perry
 * MIT License
 * 
 * http://assisticant.net
 * 
 **********************************************************************/

#if WPF

#endif
using System;

#if WPF
using Assisticant.Descriptors;
using System.Windows.Controls;
using System.Windows;
#endif

namespace Assisticant
{
    public class ViewSelector : DataTemplateSelector
    {
#if WPF
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var wrapper = item as PlatformProxy;
            var element = container as FrameworkElement;
            if (wrapper != null && element != null)
            {
                for (var type = wrapper.Instance.GetType(); type != null && type != typeof(object); type = type.BaseType)
                {
                    var template = element.TryFindResource(new DataTemplateKey(type)) as DataTemplate;
                    if (template != null)
                        return template;
                }
            }

            return base.SelectTemplate(item, container);
        }
#endif
    }
}
