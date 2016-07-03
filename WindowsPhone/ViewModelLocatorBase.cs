using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Assisticant.XAML.Wrapper;
using System.Diagnostics;

namespace Assisticant.XAML
{
    public class ViewModelLocatorBase : INotifyPropertyChanged
    {
        private class ViewModelContainer
        {
            private Computed _computed;
            private object _viewModel;
            private Action _firePropertyChanged;

            public ViewModelContainer(Action firePropertyChanged, Func<object> constructor)
            {
                _firePropertyChanged = firePropertyChanged;
                _computed = new Computed(() => _viewModel = ForView.Wrap(constructor()));
                _computed.Invalidated += () => UpdateScheduler.ScheduleUpdate(_firePropertyChanged);
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
            _designMode = DesignerProperties.IsInDesignTool;
        }

        public bool DesignMode
        {
            get { return _designMode; }
        }

        public object ViewModel(Func<object> constructor)
        {
            if (DesignMode)
                return constructor();

            string caller = new StackFrame(1).GetMethod().Name;
            if (!caller.StartsWith("get_"))
                throw new ArgumentException("Only call ViewModel from a property getter.");
            string propertyName = caller.Substring(4);

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
