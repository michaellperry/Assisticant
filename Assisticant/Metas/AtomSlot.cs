using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public class AtomSlot : MemberSlot, IUpdatable
    {
        readonly Computed _computed;
        object _value;
		bool _firePropertyChanged = false;

        internal AtomSlot(ViewProxy proxy, MemberMeta member)
			: base(proxy, member)
		{
            if (member.CanRead)
            {
                // When the property is out of date, update it from the wrapped object.
                _computed = new Computed(() => BindingInterceptor.Current.UpdateValue(this));
                // When the property becomes out of date, trigger an update.
                // The update should have lower priority than user input & drawing,
                // to ensure that the app doesn't lock up in case a large model is 
                // being updated outside the UI (e.g. via timers or the network).
                _computed.Invalidated += () => UpdateScheduler.ScheduleUpdate(this);
            }
		}

        public override void SetValue(object value)
		{
            var scheduler = UpdateScheduler.Begin();

            try
            {
                value = UnwrapValue(value);
                Member.SetValue(Instance, value);
            }
            finally
            {
                if (scheduler != null)
                {
                    foreach (IUpdatable updatable in scheduler.End())
                        updatable.UpdateNow();
                }
            }
		}

        public override object GetValue()
        {
            if (_computed.IsNotUpdating)
                _computed.OnGet();
            return _value;
        }

        internal override void UpdateValue()
        {
            object value = Member.GetValue(Instance);
            value = WrapValue(value);
            if (!Object.Equals(_value, value))
                _value = value;
            if (_firePropertyChanged)
                FirePropertyChanged();
            _firePropertyChanged = true;
        }

        void IUpdatable.UpdateNow()
        {
            _computed.OnGet();
        }
    }
}
