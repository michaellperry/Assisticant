using System;
using System.Collections.Generic;
using System.Threading;

namespace Assisticant
{
    public class UpdateScheduler
    {
        private static Action<Action> _runOnUIThread;
        private static ThreadLocal<UpdateScheduler> _currentSet = new ThreadLocal<UpdateScheduler>();
        private static List<Action> _futureUpdates = new List<Action>();

        /// <summary>
        /// Sets the delegate used to marshal scheduled updates onto the UI thread.
        /// Only the first call takes effect; later calls are ignored.
        /// </summary>
        /// <remarks>
        /// <paramref name="runOnUIThread"/> MUST always dispatch asynchronously,
        /// even when called from the UI thread already. Observable&lt;T&gt;.Value's
        /// setter raises Invalidated (which schedules the update via this delegate)
        /// before it stores the new value. If the delegate runs its action inline
        /// instead of posting it, subscribers reading through the delegate will
        /// observe the stale value. WPF's ForView uses
        /// <c>Dispatcher.BeginInvoke</c>; Android's BindingManagerExtensions hops
        /// through <c>ThreadPool.QueueUserWorkItem</c> before
        /// <c>RunOnUiThread</c>. On MAUI, prefer a queuing call such as
        /// <c>IDispatcher.DispatchDelayed(TimeSpan.Zero, action)</c> over
        /// <c>MainThread.BeginInvokeOnMainThread</c>, which runs inline when
        /// already on the main thread.
        /// </remarks>
        public static void Initialize(Action<Action> runOnUIThread)
        {
            if (_runOnUIThread == null)
            {
                _runOnUIThread = runOnUIThread;
                foreach (var update in _futureUpdates)
                    _runOnUIThread(update);
            }
        }

        public static UpdateScheduler Begin()
        {
            // If someone is already capturing the affected set,
            // let them keep that responsibility.
            if (_currentSet.Value != null)
                return null;

            UpdateScheduler currentSet = new UpdateScheduler();
            _currentSet.Value = currentSet;
            return currentSet;
        }

        public static void ScheduleUpdate(Action update)
        {
            UpdateScheduler currentSet = _currentSet.Value;
            if (currentSet != null)
                currentSet._updatables.Add(update);
            else if (_runOnUIThread != null)
                _runOnUIThread(update);
            else
                _futureUpdates.Add(update);
        }

        private List<Action> _updatables = new List<Action>();

        public IEnumerable<Action> End()
        {
            System.Diagnostics.Debug.Assert(_currentSet.Value == this);
            _currentSet.Value = null;
            return _updatables;
        }
    }
}
