﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assisticant
{
    public class ComputedJob : IUpdatable, IDisposable
    {
        Computed _computed;
        bool _running;

        public ComputedJob(Action action)
        {
            _computed = new Computed(action);
            _computed.Invalidated += () => UpdateScheduler.ScheduleUpdate(this);
        }

        public void Start()
        {
            if (_computed == null)
                throw new InvalidOperationException("Cannot restart ComputedJob");
            _running = true;
            UpdateScheduler.ScheduleUpdate(this);
        }

        public void Stop()
        {
            _running = false;
            _computed.Dispose();
            _computed = null;
        }

        public void Dispose() { Stop(); }

        void IUpdatable.UpdateNow()
        {
            if (_running)
                _computed.OnGet();
        }
    }
}
