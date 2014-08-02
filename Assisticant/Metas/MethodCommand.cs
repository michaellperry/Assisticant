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
        bool? lastCan;

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
            lastCan = _computedCan.Value;
            return lastCan.Value;
        }

        public void Execute(object parameter)
        {
            BindingInterceptor.Current.Execute(this, parameter);
        }

        internal void ContinueExecute(object parameter)
        {
            Meta.Method.Invoke(Instance, new object[0]);
        }

        private void UpdateNow()
        {
            var can = _computedCan.Value;
            if (lastCan != can && CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
