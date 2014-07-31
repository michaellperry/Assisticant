using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Windows;

namespace Assisticant
{
    public class ViewModelLocatorBase : INotifyPropertyChanged
    {
        private class ViewModelContainer
        {
            private Computed _computed;
            private object _viewModel;

            public ViewModelContainer(Action firePropertyChanged, Func<object> constructor)
            {
                _computed = new Computed(() => _viewModel = ForView.Wrap(constructor()));
                _computed.Invalidated += () => UpdateScheduler.ScheduleUpdate(firePropertyChanged);
            }

            public object ViewModel
            {
                get { _computed.OnGet(); return _viewModel; }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private IDictionary<string, ViewModelContainer> _containerByName = new Dictionary<string, ViewModelContainer>();

        private readonly bool _designMode;

        public ViewModelLocatorBase()
        {
#if WPF
            _designMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
#endif
#if UNIVERSAL
            _designMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#endif
        }

        public bool DesignMode
        {
            get { return _designMode; }
        }

        public object ViewModel(Func<object> constructor, [CallerMemberName] string propertyName = "")
        {
            if (DesignMode)
                return constructor();

            ForView.Initialize();
            ViewModelContainer container;
            if (!_containerByName.TryGetValue(propertyName, out container))
            {
                container = new ViewModelContainer(() => FirePropertyChanged(propertyName), constructor);
                _containerByName.Add(propertyName, container);
            }
            return container.ViewModel;
        }

        private void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
