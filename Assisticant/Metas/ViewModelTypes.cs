using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Assisticant.Metas
{
    public static class ViewModelTypes
    {
        static readonly Dictionary<Type, bool> _cache = new Dictionary<Type, bool>();
        static readonly Type[] _disabledByDefault = new[]
        {
            typeof(string),
            typeof(Uri),
            typeof(Cursor),
            typeof(DispatcherObject),
            typeof(INotifyPropertyChanged),
            typeof(INotifyCollectionChanged),
            typeof(ICommand),
            typeof(CommandBindingCollection),
            typeof(InputBindingCollection),
            typeof(InputScope),
            typeof(XmlLanguage),
            typeof(IEnumerable)
        };

        public static bool IsViewModel(Type type)
        {
            bool result;
            lock (_cache)
            {
                if (!_cache.TryGetValue(type, out result))
                    _cache[type] = result = IsViewModelUncached(type);
            }
            return result;
        }

        static bool IsViewModelUncached(Type type)
        {
            if (type.IsValueType || type.IsPrimitive)
                return false;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return false;
            if (_disabledByDefault.Contains(type))
                return false;
            if (type.BaseType != null && type.BaseType != typeof(object) && !IsViewModel(type.BaseType))
                return false;
            foreach (var iface in type.GetInterfaces())
                if (!IsViewModel(iface))
                    return false;
            return true;
        }
    }
}
