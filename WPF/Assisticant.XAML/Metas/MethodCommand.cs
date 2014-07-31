using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Assisticant.XAML.Metas
{
    public class MethodCommand : ICommand
    {
        public readonly object Instance;
        public readonly CommandMeta Meta;
        readonly Computed<bool> _computedCan;

        public event EventHandler CanExecuteChanged;

        public MethodCommand(object instance, CommandMeta meta)
        {
            Instance = instance;
            Meta = meta;
            if (meta.Condition != null)
            {
                _computedCan = new Computed<bool>(() => (bool)meta.Condition.GetValue(Instance));
                _computedCan.Invalidated += InvalidateCanExecute;
            }
        }

        public bool CanExecute(object parameter)
        {
            return _computedCan != null && _computedCan.Value;
        }

        public void Execute(object parameter)
        {
            BindingInterceptor.Current.Execute(this, parameter);
        }

        internal void ContinueExecute(object parameter)
        {
            Meta.Method.Invoke(Instance, new object[0]);
        }

        void InvalidateCanExecute()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
