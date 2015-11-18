using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Assisticant.Metas
{
    public class MethodCommand : ICommand
    {
        public readonly object Instance;
        public readonly CommandMeta Meta;
        readonly Computed<bool> _computedCan;
        bool? _lastCan;

        public event EventHandler CanExecuteChanged;

        public MethodCommand(object instance, CommandMeta meta)
        {
            Instance = instance;
            Meta = meta;
            if (meta.Condition != null)
            {
                _computedCan = new Computed<bool>(() => (bool)meta.Condition.GetValue(Instance));
                _computedCan.Invalidated += () => UpdateScheduler.ScheduleUpdate(UpdateNow);
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_computedCan == null)
                return true;
            return BindingInterceptor.Current.CanExecute(this, parameter);
        }

        internal bool ContinueCanExecute(object parameter)
        {
            _lastCan = _computedCan.Value;
            return _lastCan.Value;
        }

        public void Execute(object parameter)
        {
            BindingInterceptor.Current.Execute(this, parameter);
        }

        internal void ContinueExecute(object parameter)
        {
            if (Meta.HasParameter)
                Meta.Method.Invoke(Instance, new object[] { parameter });
            else
                Meta.Method.Invoke(Instance, new object[0]);
        }

        private void UpdateNow()
        {
            var can = _computedCan.Value;
            if (_lastCan != can && CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
