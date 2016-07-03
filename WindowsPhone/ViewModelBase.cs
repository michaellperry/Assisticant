using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Assisticant.XAML
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyTracker _propertyTracker;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase()
        {
            _propertyTracker = new PropertyTracker(FirePropertyChanged);
        }

        protected T Get<T>(Func<T> getMethod)
        {
            string caller = new StackFrame(1).GetMethod().Name;
            if (!caller.StartsWith("get_"))
                throw new ArgumentException("Only call Get from a property getter.");
            return Get<T>(caller.Substring(4), getMethod);
        }

        protected T Get<T>(string propertyName, Func<T> getMethod)
        {
            ForView.Initialize();
            return _propertyTracker.Get(getMethod, propertyName);
        }

        protected IEnumerable<T> GetCollection<T>(Func<IEnumerable<T>> getMethod)
        {
            string caller = new StackFrame(1).GetMethod().Name;
            if (!caller.StartsWith("get_"))
                throw new ArgumentException("Only call Get from a property getter.");
            return GetCollection<T>(caller.Substring(4), getMethod);
        }

        protected IEnumerable<T> GetCollection<T>(string propertyName, Func<IEnumerable<T>> getMethod)
        {
            ForView.Initialize();
            return _propertyTracker.GetCollection(getMethod, propertyName);
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
