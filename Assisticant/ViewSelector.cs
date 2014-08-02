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
                Type viewModelType = wrapper.Instance.GetType();
                var key = new DataTemplateKey(viewModelType);
                return element.TryFindResource(key) as DataTemplate;
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
                Type viewModelType = wrapper.Instance.GetType();
                var key = new DataTemplateKey(viewModelType);
                return TryFindResource(element, key) as DataTemplate;
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
