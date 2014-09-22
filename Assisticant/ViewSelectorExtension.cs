using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Assisticant
{
    [MarkupExtensionReturnType(typeof(ViewSelector))]
    public class DefaultSelectorExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ViewSelector();
        }
    }
}
