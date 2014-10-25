using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Assisticant.Metas
{
    public class ComputedMeta : ValuePropertyMeta
    {
        ComputedMeta(MemberMeta observable, Type unwrappedType)
            : base(observable, unwrappedType)
        {
        }

        public override void SetValue(object instance, object value)
		{
            throw new MemberAccessException();
		}

        internal static MemberMeta Intercept(MemberMeta member)
        {
            var unwrapped = UnwrapObservableType(member.MemberType, typeof(Computed<>));
            if (unwrapped != null)
                return new ComputedMeta(member, unwrapped);
            else
                return member;
        }
    }
}
