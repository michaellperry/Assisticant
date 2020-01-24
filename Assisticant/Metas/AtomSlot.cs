using System;

namespace Assisticant.Metas
{
    public class AtomSlot : MemberSlot
    {
        object _sourceValue;
        object _value;
		bool _firePropertyChanged = false;

        internal AtomSlot(ViewProxy proxy, MemberMeta member)
			: base(proxy, member)
		{
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
                    foreach (Action updatable in scheduler.End())
                        updatable();
                }
            }
		}

        public override object GetValue()
        {
            UpdateNow();
            return _value;
        }

        internal override void UpdateValue()
        {
            _sourceValue = WrapValue(Member.GetValue(Instance));
        }

        protected internal override void PublishChanges()
        {
            if (!Object.Equals(_value, _sourceValue))
            {
                _value = _sourceValue;
                if (_firePropertyChanged)
                    FirePropertyChanged();
            }
            _firePropertyChanged = true;
        }
    }
}
