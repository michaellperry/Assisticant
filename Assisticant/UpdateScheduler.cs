using System;
using System.Collections.Generic;

namespace Assisticant
{
    public class UpdateScheduler
    {
        private static Action<Action> _runOnUIThread;
        private static ThreadLocal<UpdateScheduler> _currentSet = new ThreadLocal<UpdateScheduler>();

        public static void Initialize(Action<Action> runOnUIThread)
        {
            if (_runOnUIThread == null)
            {
                _runOnUIThread = runOnUIThread;
            }
        }

        public static UpdateScheduler Begin()
        {
            // If someone is already capturing the affected set,
            // let them keep that responsibility.
            if (_currentSet.Get() != null)
                return null;

            UpdateScheduler currentSet = new UpdateScheduler();
            _currentSet.Set(currentSet);
            return currentSet;
        }

        public static void ScheduleUpdate(Action update)
        {
            UpdateScheduler currentSet = _currentSet.Get();
            if (currentSet != null)
                currentSet._updatables.Add(update);
            else if (_runOnUIThread != null)
                _runOnUIThread(update);
        }

        private List<Action> _updatables = new List<Action>();

        public IEnumerable<Action> End()
        {
            System.Diagnostics.Debug.Assert(_currentSet.Get() == this);
            _currentSet.Set(null);
            return _updatables;
        }
    }
}
