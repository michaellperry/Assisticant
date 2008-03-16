using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assisticant.Fields;

namespace Assisticant.XAML.Wrapper
{
    public class ClassMemberObservable : ClassMember
    {
        ClassMember _observable;
        PropertyInfo _valueProperty;

        public ClassMemberObservable(ClassMember observable)
            : base(observable.Name, UnwrapType(observable.UnderlyingType), observable.ComponentType)
        {
            _observable = observable;
            _valueProperty = observable.UnderlyingType.GetProperty("Value");
        }

		public override object GetObjectValue(object wrappedObject)
		{
            return _valueProperty.GetValue(_observable.GetObjectValue(wrappedObject));
		}

        public override void SetObjectValue(object wrappedObject, object value)
		{
            _valueProperty.SetValue(_observable.GetObjectValue(wrappedObject), value);
		}

		public override bool CanRead
        {
            get { return true; }
        }

		public override Type UnderlyingType
		{
            get { return UnwrapType(_observable.UnderlyingType); }
		}

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public static ClassMember Intercept(ClassMember member)
        {
            if (IsObservable(member.UnderlyingType))
                return new ClassMemberObservable(member);
            else
                return member;
        }

        public static Type UnwrapType(Type observableType)
        {
            for (Type ancestor = observableType; ancestor != typeof(object) && ancestor != null; ancestor = ancestor.BaseType)
                if (ancestor.IsGenericType && ancestor.GetGenericTypeDefinition() == typeof(Observable<>))
                    return ancestor.GenericTypeArguments[0];
            return null;
        }

        public static bool IsObservable(Type memberType)
        {
            return UnwrapType(memberType) != null;
        }
    }
}
