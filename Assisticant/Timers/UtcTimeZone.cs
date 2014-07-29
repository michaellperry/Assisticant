using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Assisticant.Timers
{
    class UtcTimeZone : FloatingTimeZone
    {
        Timer _timer;

        public override DateTime GetRawTime() { return DateTime.UtcNow; }

        public UtcTimeZone()
        {
            _timer = new Timer(Expire, null, Timeout.Infinite, -1);
        }

        protected override void ScheduleTimer(TimeSpan delay)
        {
            _timer.Change(Convert.ToInt32(Math.Max(20, delay.TotalMilliseconds)), -1);
        }

        protected override void CancelTimer()
        {
            _timer.Change(Timeout.Infinite, -1);
        }

        void Expire(object state)
        {
            _timer.Change(Timeout.Infinite, -1);
            NotifyTimerExpired();
        }
    }
}
