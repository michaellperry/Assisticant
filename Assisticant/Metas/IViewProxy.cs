using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public interface IViewProxy
    {
        object ViewModel { get; }

        void FirePropertyChanged(string name);
        IViewProxy WrapObject(object value);
    }
}
