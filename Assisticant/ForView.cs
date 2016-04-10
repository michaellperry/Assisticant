/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2010 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://updatecontrols.codeplex.com/
 * 
 **********************************************************************/

#if WPF
using System.Windows.Threading;
#endif
using System;
using Assisticant.Timers;
#if UNIVERSAL
using Windows.UI.Core;
using Windows.UI.Xaml;
#endif
#if WPF
using Assisticant.Descriptors;
using Assisticant.Metas;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows;
#endif
#if UNIVERSAL
using Assisticant.XamlTypes;
using Windows.UI.Xaml.Controls;
#endif

namespace Assisticant
{
    public static class ForView
    {
#if WPF
        private static Dispatcher _mainDispatcher;
#endif
#if UNIVERSAL
        private static CoreDispatcher _mainDispatcher;
#endif

        public static void Initialize()
        {
            // Ensure that the UpdateScheduler has the ability to run delegates
            // on the UI thread.
            if (_mainDispatcher == null)
            {
#if WPF
                _mainDispatcher = Dispatcher.CurrentDispatcher;
#endif
#if UNIVERSAL
                _mainDispatcher = Window.Current.Dispatcher;
#endif
                UpdateScheduler.Initialize(RunOnUIThread);
                FloatingTimeZone.Initialize(RunOnUIThread);
            }
        }

        /// <summary>
        /// Wrap an object to be used as the DataContext of a view.
        /// All of the properties of the object are available for
        /// data binding with automatic updates.
        /// </summary>
        /// <param name="wrappedObject">The object to wrap for the view.</param>
        /// <returns>An object suitable for data binding.</returns>
        public static object Wrap(object wrappedObject)
        {
            Initialize();
            if (wrappedObject == null)
                return null;
            return Activator.CreateInstance(typeof(PlatformProxy<>).MakeGenericType(wrappedObject.GetType()), wrappedObject);
        }

        /// <summary>
        /// Unwrap a DataContext to get back to the original object.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the object that was wrapped.</typeparam>
        /// <param name="dataContext">The DataContext previously wrapped.</param>
        /// <returns>The object originally wrapped, or null.</returns>
        public static TViewModel Unwrap<TViewModel>(object dataContext)
            where TViewModel : class
        {
            PlatformProxy proxy = dataContext as PlatformProxy;
            if (proxy != null)
                return proxy.Instance as TViewModel;
            return dataContext as TViewModel;
        }

        public static void Unwrap<TWrappedObjectType>(object dataContext, Action<TWrappedObjectType> action)
            where TWrappedObjectType : class
        {
            var viewModel = Unwrap<TWrappedObjectType>(dataContext);
            if (viewModel != null)
                action(viewModel);
        }

        public static TResult Unwrap<TWrappedObjectType, TResult>(object dataContext, Func<TWrappedObjectType, TResult> function, TResult @default = default(TResult))
            where TWrappedObjectType : class
        {
            var viewModel = Unwrap<TWrappedObjectType>(dataContext);
            if (viewModel != null)
                return function(viewModel);
            else
                return @default;
        }

#if WPF
        private static void RunOnUIThread(Action action)
        {
            if (_mainDispatcher != null)
            {
                _mainDispatcher.BeginInvoke(action, DispatcherPriority.Background);
            }
        }
#endif
#if UNIVERSAL
        private static async void RunOnUIThread(Action action)
        {
            if (_mainDispatcher != null)
            {
                await _mainDispatcher.RunAsync(
                    CoreDispatcherPriority.Low,
                    new DispatchedHandler(delegate
                    {
                        action();
                    }));
            }
        }
#endif
    }
}
