using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public abstract class MemberMeta
    {
        public readonly TypeMeta DeclaringType;
        public readonly string Name;
        public readonly Type MemberType;
        public readonly bool IsViewModelType;

        public virtual bool CanRead { get { return true; } }
        public virtual bool CanWrite { get { return true; } }

        public MemberMeta(TypeMeta owner, string name, Type type)
        {
            DeclaringType = owner;
            Name = name;
            MemberType = type;
            IsViewModelType = ViewModelTypes.IsViewModel(type);
        }

        public abstract void SetValue(object instance, object value);
        public abstract object GetValue(object instance);

        public override string ToString()
        {
            return String.Format("{0}.{1}", DeclaringType, Name);
        }
    }
}
