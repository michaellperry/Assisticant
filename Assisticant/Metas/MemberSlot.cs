using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public abstract class MemberSlot
    {
        public readonly ViewProxy Proxy;
        public readonly MemberMeta Member;
        public object Instance { get { return Proxy.Instance; } }

        protected MemberSlot(ViewProxy proxy, MemberMeta member)
        {
            Proxy = proxy;
            Member = member;
        }

        public abstract void SetValue(object value);
        public abstract object GetValue();
        internal abstract void UpdateValue();

        internal static MemberSlot Create(ViewProxy proxy, MemberMeta member)
        {
            if (member.IsCollection)
                return new CollectionSlot(proxy, member);
            else
                return new AtomSlot(proxy, member);
        }

        protected void FirePropertyChanged()
        {
            Proxy.FirePropertyChanged(Member.Name);
        }

        protected object UnwrapValue(object value)
        {
            if (!Member.IsViewModel)
                return value;
            var proxy = value as ViewProxy;
            if (proxy != null)
                return proxy.Instance;
            return value;
        }

        protected object WrapValue(object value)
        {
            if (!Member.IsCollection && !Member.IsViewModel)
                return value;
            if (value == null)
                return null;
            if (!ViewModelTypes.IsViewModel(value.GetType()))
                return value;
            return Proxy.WrapObject(value);
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", Member, Proxy.Instance);
        }
    }
}
