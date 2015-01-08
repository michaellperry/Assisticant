using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Timers
{
    /// <summary>
    /// Represents point in time relative to current time. As real wall clock ticks, FloatingDateTime floats with it, maintaining constant FloatDelta from current time.
    /// </summary>
    public class FloatingDateTime : IEquatable<FloatingDateTime>, IEquatable<DateTime>, IComparable<FloatingDateTime>, IComparable<DateTime>, IComparable
    {
        readonly FloatingTimeZone _zone;
        readonly TimeSpan _delta;

        /// <summary>
        /// Gets FloatingDateTime that always represents current UTC time, i.e. calling Snapshot on it will always return DateTime.UtcNow.
        /// </summary>
        public static FloatingDateTime UtcNow { get { return FloatingTimeZone.Utc.Now; } }
        /// <summary>
        /// Time zone of the FloatingDateTime. Every FloatingDateTime has associated FloatingTimeZone. Zone therefore cannot be null.
        /// </summary>
        public FloatingTimeZone Zone { get { return _zone; } }
        /// <summary>
        /// Position of the FloatingDateTime on time axis relative to current time. Positive values indicate time in future. Negative values indicate past time.
        /// This property is exactly equal to (this - Zone.Now).
        /// </summary>
        public TimeSpan FloatDelta { get { return _delta; } }
        /// <summary>
        /// Gets stationary DateTime that is equal to the FloatingDateTime at this point in time.
        /// The returned DateTime remains constant after it is returned while this FloatingDateTime continues to float with current time.
        /// Note that this property doesn't setup any Assisticant change notification, i.e. view models using this property won't update with fresh time.
        /// It is recommended to use Floor() in order to have Assisticant view models reevaluated when the time moves significantly.
        /// </summary>
        public DateTime Snapshot { get { return _zone.GetStableTime() + _delta; } }
        /// <summary>
        /// Extracts date component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public DateTime Date { get { return Floor(TimeSpan.FromDays(1)); } }
        /// <summary>
        /// Extracts year component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int Year { get { return GetComponent(Snapshot.Year, new DateTime(Snapshot.Year, 1, 1), new DateTime(Snapshot.Year + 1, 1, 1)); } }
        /// <summary>
        /// Extracts month component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int Month { get { return GetComponent(Snapshot.Month, new DateTime(Snapshot.Year, Snapshot.Month, 1), new DateTime(Snapshot.Year, Snapshot.Month, 1).AddMonths(1)); } }
        /// <summary>
        /// Extracts day of month component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int Day { get { return Date.Day; } }
        /// <summary>
        /// Extracts day of year component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int DayOfYear { get { return Date.DayOfYear; } }
        /// <summary>
        /// Extracts day of week component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public DayOfWeek DayOfWeek { get { return Date.DayOfWeek; } }
        /// <summary>
        /// Extracts hour component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int Hour { get { return Floor(TimeSpan.FromHours(1)).Hour; } }
        /// <summary>
        /// Extracts minute component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int Minute { get { return Floor(TimeSpan.FromMinutes(1)).Minute; } }
        /// <summary>
        /// Extracts second component from FloatingDateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when this property changes.
        /// </summary>
        public int Second { get { return Floor(TimeSpan.FromSeconds(1)).Second; } }

        internal FloatingDateTime(FloatingTimeZone zone)
        {
            _zone = zone;
        }

        FloatingDateTime(FloatingTimeZone zone, TimeSpan delta)
        {
            _zone = zone;
            _delta = delta;
        }

        /// <summary>
        /// Creates new FloatingDateTime that corresponds to point in time that is shifted into future relative to this FloatingDateTime.
        /// </summary>
        /// <param name="timespan">How much to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted into future.</returns>
        public FloatingDateTime Add(TimeSpan timespan) { return new FloatingDateTime(_zone, _delta + timespan); }
        /// <summary>
        /// Adds DroppingTimeSpan to the FloatingDateTime. The result is a stationary DateTime since the floating component of both inputs is cancelled out.
        /// </summary>
        /// <param name="timespan">DroppingTimeSpan to add to the FloatingDateTime.</param>
        /// <returns>Point in time equal to adding Snapshot properties of both inputs.</returns>
        public DateTime Add(DroppingTimeSpan timespan)
        {
            CheckZone(timespan);
            return timespan.ZeroMoment + _delta;
        }
        /// <summary>
        /// Shifts the FloatingDateTime into future by the specified number of days.
        /// </summary>
        /// <param name="days">How many days to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted by the specified number of days.</returns>
        public FloatingDateTime AddDays(double days) { return Add(TimeSpan.FromDays(days)); }
        /// <summary>
        /// Shifts the FloatingDateTime into future by the specified number of hours.
        /// </summary>
        /// <param name="hours">How many hours to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted by the specified number of hours.</returns>
        public FloatingDateTime AddHours(double hours) { return Add(TimeSpan.FromHours(hours)); }
        /// <summary>
        /// Shifts the FloatingDateTime into future by the specified number of minutes.
        /// </summary>
        /// <param name="minutes">How many minutes to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted by the specified number of minutes.</returns>
        public FloatingDateTime AddMinutes(double minutes) { return Add(TimeSpan.FromMinutes(minutes)); }
        /// <summary>
        /// Shifts the FloatingDateTime into future by the specified number of seconds.
        /// </summary>
        /// <param name="seconds">How many seconds to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted by the specified number of seconds.</returns>
        public FloatingDateTime AddSeconds(double seconds) { return Add(TimeSpan.FromSeconds(seconds)); }
        /// <summary>
        /// Shifts the FloatingDateTime into future by the specified number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">How many milliseconds to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted by the specified number of milliseconds.</returns>
        public FloatingDateTime AddMilliseconds(double milliseconds) { return Add(TimeSpan.FromMilliseconds(milliseconds)); }
        /// <summary>
        /// Shifts the FloatingDateTime into future by the specified number of ticks.
        /// </summary>
        /// <param name="ticks">How many ticks to shift the FloatingDateTime into future. Negative to shift into past.</param>
        /// <returns>FloatingDateTime shifted by the specified number of ticks.</returns>
        public FloatingDateTime AddTicks(long ticks) { return Add(TimeSpan.FromTicks(ticks)); }
        /// <summary>
        /// Creates new FloatingDateTime that corresponds to point in time that is shifted into past relative to this FloatingDateTime.
        /// </summary>
        /// <param name="timespan">How much to shift the FloatingDateTime into past. Negative to shift into future.</param>
        /// <returns>FloatingDateTime shifted into past.</returns>
        public FloatingDateTime Subtract(TimeSpan timespan) { return Add(-timespan); }
        /// <summary>
        /// Calculates difference between two instances of FloatingDateTime. The result is a constant TimeSpan, because floating components cancel out.
        /// </summary>
        /// <param name="other">FloatingDateTime to subtract from this FloatingDateTime.</param>
        /// <returns>Difference between this FloatingDateTime and the specified FloatingDateTime.</returns>
        public TimeSpan Subtract(FloatingDateTime other)
        {
            CheckZone(other);
            return _delta - other._delta;
        }
        /// <summary>
        /// Calculates difference between the FloatingDateTime and stationary DateTime. The result is a RisingTimeSpan, which tracks the difference as the time progresses.
        /// </summary>
        /// <param name="other">Stationary DateTime to subtract from the FloatingDateTime.</param>
        /// <returns>RisingTimeSpan that tracks the difference as the time progresses.</returns>
        public RisingTimeSpan Subtract(DateTime other) { return new RisingTimeSpan(_zone, other - _delta); }
        /// <summary>
        /// Subtracts RisingTimeSpan from the FloatingDateTime. The result is a stationary DateTime since the floating component of both inputs is cancelled out.
        /// </summary>
        /// <param name="timespan">RisingTimeSpan to subtract from the FloatingDateTime.</param>
        /// <returns>Point in time equal to subtracting RisingTimeSpan's Snapshot property from FloatingDateTime's Snapshot property.</returns>
        public DateTime Subtract(RisingTimeSpan timespan)
        {
            CheckZone(timespan);
            return timespan.ZeroMoment + _delta;
        }

        /// <summary>
        /// Rounds the FloatingDateTime down to the specified interval.
        /// The result is a stationary DateTime that depends on current position of FloatingDateTime on time axis.
        /// If used inside Assisticant view model, the view model will be reevaluated when the result of this method changes.
        /// </summary>
        /// <param name="interval">Rounding interval. For example, TimeSpan.FromSeconds(1) will round the FloatingDateTime down to nearest second.</param>
        /// <returns>Current position of FloatingDateTime on time axis rounded down to the specified interval.</returns>
        public DateTime Floor(TimeSpan interval)
        {
            var floored = new DateTime(Snapshot.Ticks / interval.Ticks * interval.Ticks);
            return GetComponent(floored, floored - _delta, interval);
        }

        public static FloatingDateTime operator +(FloatingDateTime left, TimeSpan right) { return left.Add(right); }
        public static DateTime operator +(FloatingDateTime left, DroppingTimeSpan right) { return left.Add(right); }
        public static DateTime operator +(DroppingTimeSpan left, FloatingDateTime right) { return right.Add(left); }
        public static FloatingDateTime operator -(FloatingDateTime left, TimeSpan right) { return left.Subtract(right); }
        public static TimeSpan operator -(FloatingDateTime left, FloatingDateTime right) { return left.Subtract(right); }
        public static RisingTimeSpan operator -(FloatingDateTime left, DateTime right) { return left.Subtract(right); }
        public static DroppingTimeSpan operator -(DateTime left, FloatingDateTime right) { return new DroppingTimeSpan(right._zone, left - right._delta); }
        public static DateTime operator -(FloatingDateTime left, RisingTimeSpan right) { return left.Subtract(right); }

        public static bool operator ==(FloatingDateTime left, FloatingDateTime right) { return left.Equals(right); }
        public static bool operator !=(FloatingDateTime left, FloatingDateTime right) { return !(left == right); }
        public static bool operator <(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) < 0; }
        public static bool operator >(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) > 0; }
        public static bool operator <=(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) <= 0; }
        public static bool operator >=(FloatingDateTime left, FloatingDateTime right) { return left.CompareTo(right) >= 0; }

        public static bool operator ==(FloatingDateTime left, DateTime right) { return left.Equals(right); }
        public static bool operator ==(DateTime left, FloatingDateTime right) { return right == left; }
        public static bool operator !=(FloatingDateTime left, DateTime right) { return !(left == right); }
        public static bool operator !=(DateTime left, FloatingDateTime right) { return right != left; }
        public static bool operator >=(FloatingDateTime left, DateTime right) { return left.GetTimer(right).IsExpired; }
        public static bool operator >=(DateTime left, FloatingDateTime right) { return right <= left; }
        public static bool operator <(FloatingDateTime left, DateTime right) { return !(left >= right); }
        public static bool operator <(DateTime left, FloatingDateTime right) { return right > left; }
        public static bool operator <=(FloatingDateTime left, DateTime right) { return left < right || left == right; }
        public static bool operator <=(DateTime left, FloatingDateTime right) { return right >= left; }
        public static bool operator >(FloatingDateTime left, DateTime right) { return left >= right && left != right; }
        public static bool operator >(DateTime left, FloatingDateTime right) { return right < left; }

        /// <summary>
        /// Compares two FloatingDateTime instances for exact equality.
        /// The result is constant since the tow FloatingDateTime instances move at the same rate and their relative position on time axis doesn't change.
        /// </summary>
        /// <param name="other">FloatingDateTime to compare against.</param>
        /// <returns>Whether FloatingDateTime instances are exactly equal.</returns>
        public bool Equals(FloatingDateTime other) { return _zone == other._zone && _delta == other._delta; }
        /// <summary>
        /// Checks equality of current position of the FloatingDateTime on time axis with stationary DateTime.
        /// If used inside Assisticant view model, the view model will be reevaluated when the result of the comparison changes.
        /// Note that it is very unlikely for FloatingDateTime to be exactly equal to fixed point in time.
        /// You should use CompareTo() methods or comparison operators to compare against time ranges.
        /// </summary>
        /// <param name="other">Stationary DateTime to compare against.</param>
        /// <returns>Whether the FloatingDateTime current position on time axis equals the stationary DateTime.</returns>
        public bool Equals(DateTime other) { return GetTimer(other).IsExpired && !GetTimer1(other).IsExpired; }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is FloatingDateTime)
                return Equals((FloatingDateTime)obj);
            if (obj is DateTime)
                return Equals((DateTime)obj);
            throw new ArgumentException();
        }
        public override int GetHashCode() { return 31 * _zone.GetHashCode() + _delta.GetHashCode(); }

        /// <summary>
        /// Compares two FloatingDateTime instances for inequality.
        /// The result is constant since the tow FloatingDateTime instances move at the same rate and their relative position on time axis doesn't change.
        /// </summary>
        /// <param name="other">FloatingDateTime to compare against.</param>
        /// <returns>
        /// Returns -1 if this FloatingDateTime is earlier than the specified FloatingDateTime,
        /// 1 if this FloatingDateTime is later than the specified FloatingDateTime,
        /// and 0 for exact equality.
        /// </returns>
        public int CompareTo(FloatingDateTime other)
        {
            CheckZone(other);
            return _delta.CompareTo(other._delta);
        }
        /// <summary>
        /// Compares current position of the FloatingDateTime on time axis with stationary DateTime for inequality.
        /// If used inside Assisticant view model, the view model will be reevaluated when the result of the comparison changes.
        /// </summary>
        /// <param name="other">FloatingDateTime to compare against.</param>
        /// <returns>
        /// Returns -1 if the FloatingDateTime is earlier than the specified DateTime,
        /// 1 if the FloatingDateTime is later than the specified DateTime,
        /// and 0 for exact equality (which is very unlikely).
        /// </returns>
        public int CompareTo(DateTime other)
        {
            return !GetTimer(other).IsExpired ? -1 : GetTimer1(other).IsExpired ? 1 : 0;
        }
        public int CompareTo(object obj)
        {
            if (obj is FloatingDateTime)
                return CompareTo((FloatingDateTime)obj);
            if (obj is DateTime)
                return CompareTo((DateTime)obj);
            throw new ArgumentException();
        }

        public override string ToString() { return Snapshot.ToString(); }

        ObservableTimer GetTimer(DateTime comparand) { return ObservableTimer.Get(_zone, comparand - _delta); }
        ObservableTimer GetTimer1(DateTime comparand) { return GetTimer(comparand.AddTicks(1)); }
        void CheckZone(FloatingDateTime other)
        {
            if (_zone != other._zone)
                throw new ArgumentException("Cannot relate ObservableDateTime from two different time zones");
        }
        void CheckZone(FloatingTimeSpan timespan)
        {
            if (_zone != timespan.Zone)
                throw new ArgumentException("Cannot relate ObservableDateTime to FloatingTimeSpan with different time zone");
        }
        T GetComponent<T>(T component, DateTime prev, TimeSpan increment)
        {
            return GetComponent(component, prev, prev + increment);
        }
        T GetComponent<T>(T component, DateTime prev, DateTime next)
        {
            ObservableTimer.Get(_zone, prev).OnGet();
            ObservableTimer.Get(_zone, next).OnGet();
            return component;
        }
    }
}
