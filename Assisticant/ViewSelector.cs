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

#if UNIVERSAL
using Windows.UI.Xaml;
#endif
#if WPF
using Assisticant.Descriptors;
using System.Windows.Controls;
using System.Windows;
#endif
#if UNIVERSAL
using Assisticant.XamlTypes;
using Windows.UI.Xaml.Controls;
using System.Reflection;
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
#if UNIVERSAL
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var wrapper = item as PlatformProxy;
            var element = container as FrameworkElement;
            if (wrapper != null && element != null)
            {
                for (var type = wrapper.Instance.GetType(); type != null && type != typeof(object); type = type.GetTypeInfo().BaseType)
                {
                    var template = TryFindResource(element, new DataTemplateKey(type)) as DataTemplate;
                    if (template != null)
                        return template;
                }
            }

            return base.SelectTemplateCore(item, container);
        }

        private static object TryFindResource(FrameworkElement element, object resourceKey)
        {
            if (element == null)
                return Application.Current.Resources[resourceKey];

            var resource = element.Resources[resourceKey];
            if (resource != null)
            {
                return resource;
            }
            return TryFindResource(element.Parent as FrameworkElement, resourceKey);
        }
#endif
    }
}
