﻿/**********************************************************************
 * 
 * Update Controls .NET
 * Copyright 2010 Michael L Perry
 * MIT License
 * 
 * http://updatecontrols.net
 * http://www.codeplex.com/updatecontrols/
 * 
 **********************************************************************/

using System;
using System.Windows.Input;

namespace Assisticant
{
    /// <summary>
    /// Creates a command object, which implements ICommand. Use the When (optional) and
    /// Do (required) methods to specify the behavior of the command. Pass lambda expressions
    /// into both methods. Use a lambda taking no parameters like this: () => &lt;condition or
    /// {statement}&gt;. Or use a lambda taking one parameter like this: param => &lt;condition
    /// or {statement}&gt;.
    /// </summary>
    public static class MakeCommand
    {
        private class Command : ICommand
        {
            // The condition under which it can execute, and the action to execute.
            private Func<bool> _canExecuteFunction;
            private Action _execute;

            // A computed flag, true when the command can be executed.
            private bool _canExecute = false;
            private Computed _depCanExecute;

            public Command(Func<bool> canExecute, Action execute)
            {
                _canExecuteFunction = canExecute;
                _execute = execute;

                // Create a computed sentry to control the "can execute" flag.
                _depCanExecute = new Computed(UpdateCanExecute);
                _depCanExecute.Invalidated += new Action(Invalidated);

                // It begins its life out-of-date, so prepare to update it.
                Invalidated();
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                // Just returning the flag. The flag gets set elsewhere.
                _depCanExecute.OnGet();
                return _canExecute;
            }

            public void Execute(object parameter)
            {
                _execute();
            }

            private void UpdateCanExecute()
            {
                // Here is where the flag gets updated. The update function
                // is executed, and the result is stored in the "can execute"
                // flag. I become dependent upon anything that the update
                // function touches.
                _canExecute = _canExecuteFunction();
            }

            private void Invalidated()
            {
                // When the "can execute" flag is invalidated, we need to queue
                // up a call to update it. This will cause the UI thread to
                // call TriggerUpdate (below) when everything settles down.
                UpdateScheduler.ScheduleUpdate(UpdateNow);
            }

            private void UpdateNow()
            {
                // The "can execute" flag is now out-of-date. We need to update it.
                _depCanExecute.OnGet();

                // Now that it is up-to-date again, we need to notify anybody bound
                // to this command that the flag has changed.
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, new EventArgs());
            }
        }

        public class Condition
        {
            private Func<bool> _canExecute;

            public Condition(Func<bool> canExecute)
            {
                _canExecute = canExecute;
            }

            /// <summary>
            /// Specify an action to execute when the command is invoked. The action is a lambda
            /// taking no parameters and performing a statement. The syntax looks like: () => { DoSomething(); }
            /// </summary>
            /// <param name="execute">A lambda expression taking no parameters and performing a statement. The
            /// syntax looks like: () => { DoSomething(); }</param>
            /// <returns>A command that does that.</returns>
            public ICommand Do(Action execute)
            {
                return new Command(_canExecute, execute);
            }

            public ICommand Do(Action<object> execute)
            {
                return new Command(_canExecute, () => execute(null));
            }
        }

        /// <summary>
        /// Specify a condition under which the command can be executed. Controls bound to the
        /// command will only be enabled when this condition is true. The condition is a lambda
        /// taking no parameters and returning a boolean. The syntax looks like: () => SelectedThing != null
        /// </summary>
        /// <param name="condition">A lambda expression taking no parameters and returing a boolean: () => SelectedThing != null</param>
        /// <returns>An object that you can add .Do to.</returns>
        public static Condition When(Func<bool> condition)
        {
            return new Condition(condition);
        }

        public static Condition When(Func<object, bool> condition)
        {
            return new Condition(() => condition(null));
        }

        /// <summary>
        /// Specify an action to execute when the command is invoked. The action is a lambda
        /// taking no parameters and performing a statement. The syntax looks like: () => { DoSomething(); }
        /// </summary>
        /// <param name="execute">A lambda expression taking no parameters and performing a statement. The
        /// syntax looks like: () => { DoSomething(); }</param>
        /// <returns>A command that does that.</returns>
        public static ICommand Do(Action execute)
        {
            return new Command(() => true, execute);
        }
    }
}
