using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public class ObservableMeta : MemberMeta
    {
        public readonly MemberMeta UnderlyingMember;
        readonly PropertyInfo _valueProperty;

        ObservableMeta(MemberMeta observable, Type unwrappedType)
            : base(observable.DeclaringType, observable.Name, unwrappedType)
        {
            UnderlyingMember = observable;
            _valueProperty = observable.MemberType.GetPropertyPortable("Value");
        }

		public override object GetValue(object instance)
		{
            return _valueProperty.GetValue(UnderlyingMember.GetValue(instance));
		}

        public override void SetValue(object instance, object value)
		{
            _valueProperty.SetValue(UnderlyingMember.GetValue(instance), value);
		}

        internal static MemberMeta Intercept(MemberMeta member)
        {
            var unwrapped = UnwrapObservableType(member.MemberType);
            if (unwrapped != null)
                return new ObservableMeta(member, unwrapped);
            else
                return null;
        }

        static Type UnwrapObservableType(Type observable)
        {
            for (Type ancestor = observable; ancestor != typeof(object) && ancestor != null; ancestor = ancestor.BaseTypePortable())
                if (ancestor.IsGenericTypePortable() && ancestor.GetGenericTypeDefinition() == typeof(Observable<>))
                    return ancestor.GenericTypeArguments[0];
            return null;
        }
    }
}
