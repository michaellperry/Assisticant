using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Assisticant.Metas
{
    public class CommandMeta : MemberMeta
    {
        public readonly MethodInfo Method;
        public readonly MemberMeta Condition;

        public override bool CanWrite { get { return false; } }

        CommandMeta(TypeMeta owner, MethodInfo method, MemberMeta condition)
            : base(owner, method.Name, typeof(ICommand))
        {
            Method = method;
            Condition = condition;
        }

        public override object GetValue(object instance)
        {
            return new MethodCommand(instance, this);
        }

        public override void SetValue(object instance, object value)
        {
            throw new NotSupportedException();
        }

        internal static IEnumerable<MemberMeta> GetAll(TypeMeta owner, IEnumerable<MemberMeta> properties)
        {
            var conditions = (from property in properties
                              where property.MemberType == typeof(bool) && property.Name.StartsWith("Can")
                              select property).ToList();
            return from method in owner.Type.GetMethods()
                   where method.ReturnType == typeof(void) && method.GetParameters().Length == 0
                   select new CommandMeta(owner, method, conditions.FirstOrDefault(c => c.Name == "Can" + method.Name));
        }
    }
}
