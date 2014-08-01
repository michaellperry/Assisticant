using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public class PropertyMeta : MemberMeta
    {
        public readonly PropertyInfo Property;

        public override bool CanRead { get { return Property.CanRead; } }
        public override bool CanWrite { get { return Property.CanWrite; } }

        PropertyMeta(TypeMeta owner, PropertyInfo property)
            : base(owner, property.Name, property.PropertyType)
        {
            Property = property;
        }

        public override object GetValue(object instance)
        {
            return Property.GetValue(instance, null);
        }

        public override void SetValue(object instance, object value)
        {
            Property.SetValue(instance, value, null);
        }

        internal static IEnumerable<MemberMeta> GetAll(TypeMeta owner)
        {
            return from property in owner.Type.GetPropertiesPortable()
                   where property.CanRead && property.GetGetMethodPortable().IsPublic && !property.GetGetMethodPortable().IsStatic
                   select new PropertyMeta(owner, property);
        }
    }
}
