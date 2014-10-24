using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public class TypeMeta
    {
        public readonly Type Type;
        public readonly MemberMeta[] Members;
        static readonly Dictionary<Type, TypeMeta> _cache = new Dictionary<Type, TypeMeta>();

        TypeMeta(Type type)
        {
            Type = type;
            var properties = PropertyMeta.GetAll(this).Concat(FieldMeta.GetAll(this)).Select(ValuePropertyMeta.Intercept).ToList();
            Members = properties.Concat(CommandMeta.GetAll(this, properties)).ToArray();
        }

        public static TypeMeta Get(Type type)
        {
            TypeMeta result;
            lock (_cache)
            {
                if (!_cache.TryGetValue(type, out result))
                    _cache[type] = result = new TypeMeta(type);
            }
            return result;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
