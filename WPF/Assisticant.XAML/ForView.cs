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

using System.Windows.Threading;
using System;
using Assisticant.Timers;
using Assisticant.XAML.Descriptors;

namespace Assisticant.XAML
{
    public static class ForView
    {
        private static Dispatcher _mainDispatcher;

        public static void Initialize()
        {
            // Ensure that the UpdateScheduler has the ability to run delegates
            // on the UI thread.
            if (_mainDispatcher == null)
            {
                _mainDispatcher = Dispatcher.CurrentDispatcher;
            }
            UpdateScheduler.Initialize(RunOnUIThread);
            FloatingTimeZone.Initialize(RunOnUIThread);
        }

        /// <summary>
        /// Wrap an object to be used as the DataContext of a view.
        /// All of the properties of the object are available for
        /// data binding with automatic updates.
        /// </summary>
        /// <param name="wrappedObject">The object to wrap for the view.</param>
        /// <typeparam name="TWrappedObjectType">!!!DO NOT SPECIFY!!!</typeparam>
        /// <returns>An object suitable for data binding.</returns>
        public static object Wrap<TWrappedObjectType>(TWrappedObjectType wrappedObject)
        {
            Initialize();
            if (wrappedObject == null)
                return null;
            return Activator.CreateInstance(typeof(ViewProxy<>).MakeGenericType(wrappedObject.GetType()), wrappedObject);
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
            ViewProxy proxy = dataContext as ViewProxy;
            if (proxy != null)
                return proxy.ViewModel as TViewModel;
            return dataContext as TViewModel;
        }

        private static void RunOnUIThread(Action action)
        {
            if (_mainDispatcher != null)
            {
                _mainDispatcher.BeginInvoke(action, DispatcherPriority.Background);
            }
        }
    }
}
