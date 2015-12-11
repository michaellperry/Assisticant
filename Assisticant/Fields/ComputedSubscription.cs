using System;

namespace Assisticant.Fields
{
    public class ComputedSubscription
    {
        private readonly Computed _computed;
        private readonly Func<object, object> _update;

        private object _priorState;
        
        public ComputedSubscription(Computed computed, Func<object, object> update, object initialState)
        {
            _computed = computed;
            _update = update;
            _priorState = initialState;

            _computed.Invalidated += Computed_Invalidated;
            Computed_Invalidated();
        }

        public void Unsubscribe()
        {
            _computed.Invalidated -= Computed_Invalidated;
        }

        private void Computed_Invalidated()
        {
            UpdateScheduler.ScheduleUpdate(delegate ()
            {
                _priorState = _update(_priorState);
            });
        }
    }
}
