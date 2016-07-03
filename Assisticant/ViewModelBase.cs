using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Assisticant
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyTracker _propertyTracker;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase()
        {
            _propertyTracker = new PropertyTracker(FirePropertyChanged);
        }

        protected T Get<T>(Func<T> getMethod, [CallerMemberName] string propertyName = "")
        {
            ForView.Initialize();
            return _propertyTracker.Get(getMethod, propertyName);
        }

        protected IEnumerable<T> GetCollection<T>(Func<IEnumerable<T>> getMethod, [CallerMemberName] string propertyName = "")
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
