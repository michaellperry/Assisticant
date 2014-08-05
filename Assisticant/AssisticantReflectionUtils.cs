using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant
{
    public static class AssisticantReflectionUtils
    {
#if UNIVERSAL
        public static Delegate CreateDelegatePortable(this MethodInfo method, Type delegateType, object target) { return method.CreateDelegate(delegateType, target); }
        public static bool IsValueTypePortable(this Type type) { return type.GetTypeInfo().IsValueType; }
        public static bool IsPrimitivePortable(this Type type) { return type.GetTypeInfo().IsPrimitive; }
        public static bool IsGenericTypePortable(this Type type) { return type.GetTypeInfo().IsGenericType; }
        public static Type BaseTypePortable(this Type type) { return type.GetTypeInfo().BaseType; }
        public static IEnumerable<Type> GetInterfacesPortable(this Type type)
        {
            var collector = new List<Type>();
            foreach (var iface in type.GetTypeInfo().ImplementedInterfaces)
            {
                collector.Add(iface);
                foreach (var nested in iface.GetInterfacesPortable())
                    collector.Add(iface);
            }
            var parent = type.GetTypeInfo().BaseType;
            if (parent != null)
                foreach (var inherited in parent.GetInterfacesPortable())
                    collector.Add(inherited);
            return collector.Distinct();
        }
        public static IEnumerable<Type> GetGenericArgumentsPortable(this Type type) { return type.GenericTypeArguments; }
        public static PropertyInfo GetPropertyPortable(this Type type, string name) { return type.GetMemberPortable(t => t.GetDeclaredProperty(name)); }
        public static MethodInfo GetMethodPortable(this Type type, string name) { return type.GetMemberPortable(t => t.GetDeclaredMethod(name)); }
        public static T GetMemberPortable<T>(this Type type, Func<TypeInfo, T> getter)
            where T : class
        {
            var member = getter(type.GetTypeInfo());
            if (member != null)
                return member;
            var parent = type.GetTypeInfo().BaseType;
            if (parent != null)
                return parent.GetMemberPortable(getter);
            return null;
        }
        public static bool IsAssignableFromPortable(this Type type, Type other) { return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo()); }
        public static IEnumerable<PropertyInfo> GetPropertiesPortable(this Type type) { return type.GetMembersPortable(t => t.DeclaredProperties); }
        public static IEnumerable<FieldInfo> GetFieldsPortable(this Type type) { return type.GetMembersPortable(t => t.DeclaredFields); }
        public static IEnumerable<MethodInfo> GetMethodsPortable(this Type type) { return type.GetMembersPortable(t => t.DeclaredMethods); }
        static IEnumerable<T> GetMembersPortable<T>(this Type type, Func<TypeInfo, IEnumerable<T>> getDeclared)
        {
            foreach (var member in getDeclared(type.GetTypeInfo()))
                yield return member;
            var parent = type.GetTypeInfo().BaseType;
            if (parent != null)
                foreach (var inherited in parent.GetMembersPortable(getDeclared))
                    yield return inherited;
        }
        public static MethodInfo GetGetMethodPortable(this PropertyInfo property) { return property.GetMethod; }
        public static bool IsClassPortable(this Type type) { return type.GetTypeInfo().IsClass; }
#else
        static readonly BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        public static Delegate CreateDelegatePortable(this MethodInfo method, Type delegateType, object target) { return Delegate.CreateDelegate(delegateType, target, method); }
        public static bool IsValueTypePortable(this Type type) { return type.IsValueType; }
        public static bool IsPrimitivePortable(this Type type) { return type.IsPrimitive; }
        public static bool IsGenericTypePortable(this Type type) { return type.IsGenericType; }
        public static Type BaseTypePortable(this Type type) { return type.BaseType; }
        public static IEnumerable<Type> GetInterfacesPortable(this Type type) { return type.GetInterfaces(); }
        public static IEnumerable<Type> GetGenericArgumentsPortable(this Type type) { return type.GetGenericArguments(); }
        public static PropertyInfo GetPropertyPortable(this Type type, string name) { return type.GetProperty(name, AllFlags); }
        public static MethodInfo GetMethodPortable(this Type type, string name) { return type.GetMethod(name, AllFlags); }
        public static bool IsAssignableFromPortable(this Type type, Type other) { return type.IsAssignableFrom(other); }
        public static IEnumerable<PropertyInfo> GetPropertiesPortable(this Type type) { return type.GetProperties(AllFlags); }
        public static IEnumerable<FieldInfo> GetFieldsPortable(this Type type) { return type.GetFields(AllFlags); }
        public static IEnumerable<MethodInfo> GetMethodsPortable(this Type type) { return type.GetMethods(AllFlags); }
        public static MethodInfo GetGetMethodPortable(this PropertyInfo property) { return property.GetGetMethod(true); }
        public static bool IsClassPortable(this Type type) { return type.IsClass; }
#endif
    }
}
