using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using SystemStaticExtension = System.Windows.Markup.StaticExtension;

namespace Assisticant
{
    [MarkupExtensionReturnType(typeof(object))]
    public class StaticExtension : MarkupExtension
    {
        readonly SystemStaticExtension _inner = new SystemStaticExtension();

        [ConstructorArgument("member")]
        public string Member { get { return _inner.Member; } set { _inner.Member = value; } }

        public Type MemberType { get { return _inner.MemberType; } set { _inner.MemberType = value; } }

        public StaticExtension()
        {
        }

        public StaticExtension(string member)
        {
            Member = member;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return ForView.Wrap(_inner.ProvideValue(serviceProvider));
        }
    }
}
