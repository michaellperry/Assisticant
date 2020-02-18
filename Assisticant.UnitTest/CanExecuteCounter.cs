using System;
using System.Windows.Input;

namespace Assisticant.UnitTest.WPF
{
    public class CanExecuteCounter : IDisposable
    {
        private ICommand _command;
        private int _count;

        public CanExecuteCounter(ICommand command)
        {
            _command = command;
            _command.CanExecuteChanged += Command_CanExecuteChanged;
        }

        public void Dispose()
        {
            _command.CanExecuteChanged -= Command_CanExecuteChanged;
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            ++_count;
        }

        public int Count => _count;
    }
}
