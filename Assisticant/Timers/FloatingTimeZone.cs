using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assisticant.Timers
{
    public abstract class FloatingTimeZone
    {
        Dictionary<DateTime, WeakReference> _timers = new Dictionary<DateTime, WeakReference>();
        int _purgePressure;
        HashSet<ObservableTimer> _forth = new HashSet<ObservableTimer>();
        HashSet<ObservableTimer> _back = new HashSet<ObservableTimer>();
        DateTime _schedule;
        DateTime? _stable;
        FloatingDateTime _now;
        static Action<Action> _post = action => SynchronizationContext.Current.Post(state => action(), null);

        public static readonly FloatingTimeZone Utc = new UtcTimeZone();

        public FloatingDateTime Now { get { return _now; } }

        protected FloatingTimeZone()
        {
            _now = new FloatingDateTime(this);
        }

        public abstract DateTime GetRawTime();
        protected abstract void ScheduleTimer(TimeSpan delay);
        protected abstract void CancelTimer();

        public static void Initialize(Action<Action> post) { _post = post; }

        public DateTime GetStableTime()
        {
            if (_stable == null)
            {
                _post(() => _stable = null);
                _stable = GetRawTime();
            }
            return _stable.Value;
        }

        protected void NotifyTimerExpired()
        {
            _post(() =>
            {
                _schedule = DateTime.MinValue;
                ScheduleNext();
            });
        }

        internal ObservableTimer GetTimer(DateTime time)
        {
            if (_timers.ContainsKey(time))
            {
                var cached = _timers[time].Target as ObservableTimer;
                if (cached != null)
                    return cached;
            }
            if (2 * _purgePressure >= _timers.Count)
            {
                foreach (var key in _timers.Keys.ToList())
                    if (!_timers[key].IsAlive)
                        _timers.Remove(key);
                _purgePressure = 0;
            }
            var created = new ObservableTimer(this, time);
            _timers[time] = new WeakReference(created);
            ++_purgePressure;
            return created;
        }

        internal void Enqueue(ObservableTimer timer, bool expired)
        {
            (expired ? _back : _forth).Add(timer);
            ScheduleNext();
        }

        internal void Dequeue(ObservableTimer timer)
        {
            _forth.Remove(timer);
            _back.Remove(timer);
        }

        void ScheduleNext()
        {
            var now = GetStableTime();
            foreach (var timer in _back.Where(t => t.ExpirationTime > now).ToList())
            {
                _back.Remove(timer);
                _forth.Add(timer);
                timer.Expire(false);
            }
            foreach (var timer in _back.Where(t => t.ExpirationTime <= now).ToList())
            {
                _forth.Remove(timer);
                _back.Add(timer);
                timer.Expire(true);
            }
            var head = _forth.OrderBy(t => t.ExpirationTime).FirstOrDefault();
            if (head == null && _back.Count == 0)
            {
                if (_schedule != DateTime.MinValue)
                {
                    _schedule = DateTime.MinValue;
                    CancelTimer();
                }
            }
            else if (head == null)
            {
                if (_schedule != DateTime.MaxValue)
                {
                    _schedule = DateTime.MaxValue;
                    ScheduleTimer(TimeSpan.FromSeconds(1));
                }
            }
            else if (head.ExpirationTime != _schedule)
            {
                _schedule = head.ExpirationTime;
                ScheduleTimer(new TimeSpan(Math.Min((_schedule - now).Ticks, TimeSpan.FromSeconds(1).Ticks)));
            }
        }
    }
}
