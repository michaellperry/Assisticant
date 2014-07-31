using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.XAML.Metas
{
    public abstract class MemberSlot
    {
        public readonly IViewProxy Proxy;
        public readonly MemberMeta Member;
        public object Instance { get { return Proxy.ViewModel; } }

        protected MemberSlot(IViewProxy proxy, MemberMeta member)
        {
            Proxy = proxy;
            Member = member;
        }

        public abstract void SetValue(object value);
        public abstract object GetValue();
        internal abstract void UpdateValue();

        internal static MemberSlot Create(IViewProxy proxy, MemberMeta member)
        {
            if (typeof(IEnumerable).IsAssignableFrom(member.MemberType))
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
            if (!Member.IsViewModelType)
                return value;
            var proxy = value as IViewProxy;
            if (proxy != null)
                return proxy.ViewModel;
            return value;
        }

        protected object WrapValue(object value)
        {
            if (!Member.IsViewModelType)
                return value;
            if (value == null)
                return null;
            if (!ViewModelTypes.IsViewModel(value.GetType()))
                return value;
            return Proxy.WrapObject(value);
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", Member, Proxy.ViewModel);
        }
    }
}
