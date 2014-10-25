using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Assisticant.Metas
{
    public abstract class ValuePropertyMeta : MemberMeta
    {
        public readonly MemberMeta UnderlyingMember;
        protected readonly PropertyInfo _valueProperty;

        protected ValuePropertyMeta(MemberMeta observable, Type unwrappedType)
            : base(observable.DeclaringType, observable.Name, unwrappedType)
        {
            UnderlyingMember = observable;
            _valueProperty = observable.MemberType.GetPropertyPortable("Value");
        }

		public override object GetValue(object instance)
		{
            return _valueProperty.GetValue(UnderlyingMember.GetValue(instance), null);
		}

        internal static MemberMeta InterceptAny(MemberMeta member)
        {
            return ComputedMeta.Intercept(ObservableMeta.Intercept(member));
        }

        protected static Type UnwrapObservableType(Type observable, Type wrapper)
        {
            for (Type ancestor = observable; ancestor != typeof(object) && ancestor != null; ancestor = ancestor.BaseTypePortable())
                if (ancestor.IsGenericTypePortable() && ancestor.GetGenericTypeDefinition() == wrapper)
                    return ancestor.GetGenericArgumentsPortable().First();
            return null;
        }
    }
}
