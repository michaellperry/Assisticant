using Assisticant.Metas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;

namespace Assisticant.XamlTypes
{
    public class ProxyXamlType : IXamlType
    {
        readonly TypeMeta _meta;
        readonly Type _underlyingType;
        readonly Dictionary<string, ProxyXamlMember> _members;
        static readonly Dictionary<Type, ProxyXamlType> _cache = new Dictionary<Type, ProxyXamlType>();

        public IXamlType BaseType
        {
            get { return null; }
        }

        public IXamlMember ContentProperty
        {
            get { return null; }
        }

        public string FullName
        {
            get { return _meta.Type.FullName; }
        }

        public bool IsArray
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsBindable
        {
            get { return true; }
        }

        public bool IsCollection
        {
            get { return false; }
        }

        public bool IsConstructible
        {
            get { return false; }
        }

        public bool IsDictionary
        {
            get { return false; }
        }

        public bool IsMarkupExtension
        {
            get { return false; }
        }

        public IXamlType ItemType
        {
            get { throw new NotImplementedException(); }
        }

        public IXamlType KeyType
        {
            get { throw new NotImplementedException(); }
        }

        public Type UnderlyingType
        {
            get { return _underlyingType; }
        }

        ProxyXamlType(TypeMeta meta)
        {
            _meta = meta;
            _underlyingType = typeof(PlatformProxy<>).MakeGenericType(meta.Type);
            _members = meta.Members.Select(m => new ProxyXamlMember(this, m)).ToDictionary(m => m.Name);
        }

        public static ProxyXamlType Get(Type type)
        {
            var meta = TypeMeta.Get(type);
            lock (_cache)
            {
                ProxyXamlType result;
                if (!_cache.TryGetValue(type, out result))
                    _cache[type] = result = new ProxyXamlType(meta);
                return result;
            }
        }

        public object ActivateInstance()
        {
            throw new NotImplementedException();
        }

        public void AddToMap(object instance, object key, object value)
        {
            throw new NotImplementedException();
        }

        public void AddToVector(object instance, object value)
        {
            throw new NotImplementedException();
        }

        public object CreateFromString(string value)
        {
            throw new NotImplementedException();
        }

        public IXamlMember GetMember(string name)
        {
            ProxyXamlMember member;
            _members.TryGetValue(name, out member);
            return member;
        }

        public void RunInitializer()
        {
        }
    }
}
