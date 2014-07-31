using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reflection;
#if WPF
using System.Windows.Markup;
using System.Windows.Threading;
#endif
#if UNIVERSAL
using Windows.UI.Xaml;
#endif

namespace Assisticant.Metas
{
    public static class ViewModelTypes
    {
        static readonly Dictionary<Type, bool> _cache = new Dictionary<Type, bool>();
        static readonly Type[] _disabledByDefault = new[]
        {
            typeof(string),
            typeof(Uri),
#if WPF
            typeof(Cursor),
            typeof(DispatcherObject),
#endif
#if UNIVERSAL
            typeof(DependencyObject),
#endif
            typeof(INotifyPropertyChanged),
            typeof(INotifyCollectionChanged),
            typeof(ICommand),
#if WPF
            typeof(CommandBindingCollection),
            typeof(InputBindingCollection),
            typeof(InputScope),
            typeof(XmlLanguage),
#endif
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
            if (type.IsValueTypePortable() || type.IsPrimitivePortable())
                return false;
            if (type.IsGenericTypePortable() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return false;
            if (_disabledByDefault.Contains(type))
                return false;
            var parent = type.BaseTypePortable();
            if (parent != null && parent != typeof(object) && !IsViewModel(parent))
                return false;
            foreach (var iface in type.GetInterfacesPortable())
                if (!IsViewModel(iface))
                    return false;
            return true;
        }
    }
}
