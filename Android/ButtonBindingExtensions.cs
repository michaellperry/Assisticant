using Android.Widget;
using System;

namespace Assisticant.Binding
{
    /// <summary>
    /// Button binding extensions.
    /// </summary>
    public static class ButtonBindingExtensions
    {
        class ButtonClickSubscription : IInputSubscription
        {
            private Button _control;
            private Action _action;

            public ButtonClickSubscription(Button control, Action action)
            {
                _control = control;
                _action = action;
            }

            public void Subscribe()
            {
                _control.Click += ButtonClick;
            }

            public void Unsubscribe()
            {
                _control.Click -= ButtonClick;
            }

            private void ButtonClick(object sender, EventArgs e)
            {
                _action();
            }
        }

        /// <summary>
        /// Bind a button's command to an action.
        /// </summary>
        /// <param name="bindings">The binding manager.</param>
        /// <param name="control">The button.</param>
        /// <param name="action">The action to perform when the button is tapped.</param>
        public static void BindCommand(this BindingManager bindings, Button control, Action action)
        {
            bindings.Bind(new ButtonClickSubscription(control, action));
        }

        /// <summary>
        /// Bind a button's command and Enabled property to an action and a condition.
        /// </summary>
        /// <param name="bindings">The binding manager.</param>
        /// <param name="control">The button.</param>
        /// <param name="action">The ation to perform when the button is tapped.</param>
        /// <param name="condition">The condition that controls when the button is enabled.</param>
        public static void BindCommand(this BindingManager bindings, Button control, Action action, Func<bool> condition)
        {
            bindings.Bind(condition, b => control.Enabled = b, new ButtonClickSubscription(control, action));
        }
    }
}