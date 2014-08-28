using System;

namespace Assisticant.Fields
{
    public class ComputedSubscription
    {
        private readonly Computed _computed;
        private readonly Action _update;
        
        public ComputedSubscription(Computed computed, Action update)
        {
            _computed = computed;
            _update = update;

            _computed.Invalidated += Computed_Invalidated;
            Computed_Invalidated();
        }

        public void Unsubscribe()
        {
            _computed.Invalidated -= Computed_Invalidated;
        }

        private void Computed_Invalidated()
        {
            UpdateScheduler.ScheduleUpdate(_update);
        }
    }
}
