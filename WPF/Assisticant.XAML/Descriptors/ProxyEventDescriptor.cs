using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.XAML.Descriptors
{
    public class ProxyEventDescriptor : EventDescriptor
    {
        readonly EventInfo _eventInfo;

        public ProxyEventDescriptor(EventInfo eventInfo)
            : base(eventInfo.Name, null)
        {
            _eventInfo = eventInfo;
        }

        public override void AddEventHandler(object proxy, Delegate value)
        {
            // Add the event handler to the wrapped object.
            _eventInfo.AddEventHandler(Unwrap(proxy), value);
        }

        public override void RemoveEventHandler(object proxy, Delegate value)
        {
            // Remove the event handler from the wrapped object.
            _eventInfo.RemoveEventHandler(Unwrap(proxy), value);
        }

        public override Type ComponentType
        {
            get { return _eventInfo.DeclaringType; }
        }

        public override Type EventType
        {
            get { return _eventInfo.EventHandlerType; }
        }

        public override bool IsMulticast
        {
            get { return _eventInfo.IsMulticast; }
        }

        private static object Unwrap(object proxy)
        {
            return ((ViewProxy)proxy).ViewModel;
        }
    }
}
