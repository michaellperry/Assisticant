using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.XAML.Metas
{
    public class FieldMeta : MemberMeta
    {
        public readonly FieldInfo Field;

        public override bool CanWrite { get { return !Field.IsInitOnly; } }

        FieldMeta(TypeMeta owner, FieldInfo field)
            : base(owner, field.Name, field.FieldType)
        {
            Field = field;
        }

        public override object GetValue(object instance)
        {
            return Field.GetValue(instance);
        }

        public override void SetValue(object instance, object value)
        {
            if (!Field.IsInitOnly)
                Field.SetValue(instance, value);
            else
                throw new MemberAccessException();
        }

        internal static IEnumerable<MemberMeta> GetAll(TypeMeta owner)
        {
            return from property in owner.Type.GetFields()
                   select new FieldMeta(owner, property);
        }
    }
}
