using System;

namespace Assisticant.Metas
{
    public class PassThroughSlot : MemberSlot
    {
        public PassThroughSlot(ViewProxy proxy, MemberMeta member) : base(proxy, member)
        {
        }

        public override void SetValue(object value)
        {
            Member.SetValue(Instance, value);
        }

        public override object GetValue()
        {
            return Member.GetValue(Instance);
        }

        protected internal override void PublishChanges()
        {
        }

        internal override void UpdateValue()
        {
        }
    }
}
