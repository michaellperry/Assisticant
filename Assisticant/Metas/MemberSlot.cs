using System;
using System.Collections.Generic;
using System.Linq;

namespace Assisticant.Metas
{
    public abstract class MemberSlot
    {
        public readonly ViewProxy Proxy;
        public readonly MemberMeta Member;
        protected readonly Computed _computed;
        public object Instance { get { return Proxy.Instance; } }

        protected MemberSlot(ViewProxy proxy, MemberMeta member)
        {
            Proxy = proxy;
            Member = member;
            if (member.CanRead)
            {
                // When the property is out of date, update it from the wrapped object.
                _computed = new Computed(() => BindingInterceptor.Current.UpdateValue(this));
                // When the property becomes out of date, trigger an update.
                // The update should have lower priority than user input & drawing,
                // to ensure that the app doesn't lock up in case a large model is 
                // being updated outside the UI (e.g. via timers or the network).
                _computed.Invalidated += () => UpdateScheduler.ScheduleUpdate(UpdateNow);
            }
        }

        public abstract void SetValue(object value);
        public abstract object GetValue();
        internal abstract void UpdateValue();
        protected internal abstract void PublishChanges();

        internal static MemberSlot Create(ViewProxy proxy, MemberMeta member)
        {
            if (member.IsCollection)
            {
                if (member.IsObservableCollection)
                {
                    return new PassThroughSlot(proxy, member);
                }
                else if (member.IsList)
				{
					return new ListSlot(proxy, member);
				}
				else
                {
#if NETFRAMEWORK
                    if (BindingListSlot.AppliesTo(member))
                    {
                        return new BindingListSlot(proxy, member);
                    }
                    else
                    {
                        return new CollectionSlot(proxy, member);
                    }
#else
                    return new CollectionSlot(proxy, member);
#endif
                }
            }
            else
            {
                return new AtomSlot(proxy, member);
            }
        }

        protected void FirePropertyChanged()
        {
            Proxy.FirePropertyChanged(Member.Name);
        }

        protected object UnwrapValue(object value)
        {
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

        protected void UpdateNow()
        {
            if (_computed.IsNotUpdating && !_computed.IsUpToDate)
            {
                _computed.OnGet();

                // Publish changes to higher priority members first.
                // For example, publish ItemsSource changes before SelectedItem.
                foreach (MemberSlot earlierMember in Member.EarlierMembers.Select(m => Proxy.LookupSlot(m)))
                {
                    earlierMember.UpdateNow();
                }

                // Update the GUI outside of the update method
                // so we don't take a dependency on template bindings.
                Proxy.Notify(() => PublishChanges());
            }
        }

        public override string ToString()
        {
            return String.Format("{0}({1})", Member, Proxy.Instance);
        }
    }
}
