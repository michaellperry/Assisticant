using Android.App;
using System;
using System.Threading;

namespace Assisticant.Binding
{
    /// <summary>
    /// Binding manager extensions.
    /// </summary>
    public static class BindingManagerExtensions
    {
        /// <summary>
        /// Initialize the binding manager for an activity.
        /// </summary>
        /// <param name="bindings">The binding manager for the activity.</param>
        /// <param name="activity">The activity that owns the binding manager.</param>
        public static void Initialize(this BindingManager bindings, Activity activity)
        {
            UpdateScheduler.Initialize(action =>
            {
                ThreadPool.QueueUserWorkItem(delegate(Object obj)
                {
                    activity.RunOnUiThread(action);
                });
            });
        }
    }
}
