using Assisticant.XAML.Metas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.XAML.Descriptors
{
    [DebuggerDisplay("ForView.Wrap({Instance})")]
    public abstract class ViewProxy : IViewProxy, INotifyPropertyChanged, IDataErrorInfo, IEditableObject
    {
        public readonly object Instance;
        readonly MemberSlot[] _slots;

        public object ViewModel { get { return Instance; } }

        public string Error
        {
            get
            {
                var errorInfo = Instance as IDataErrorInfo;
                return errorInfo != null ? errorInfo.Error : null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                var errorInfo = Instance as IDataErrorInfo;
                return errorInfo != null ? errorInfo[columnName] : null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewProxy(object instance)
        {
            Instance = instance;
            _slots = (from member in GetTypeDescriptor().Meta.Members
                      select MemberSlot.Create(this, member)).ToArray();
        }

        public abstract ProxyTypeDescriptor GetTypeDescriptor();

        public MemberSlot LookupSlot(MemberMeta member)
        {
            return _slots.FirstOrDefault(s => s.Member == member);
        }

        public void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public IViewProxy WrapObject(object value)
        {
            if (value == null)
                return null;
            return (ViewProxy)Activator.CreateInstance(typeof(ViewProxy<>).MakeGenericType(value.GetType()), value);
        }

        public void BeginEdit()
        {
            var editable = Instance as IEditableObject;
            if (editable != null)
                editable.BeginEdit();
        }

        public void CancelEdit()
        {
            var editable = Instance as IEditableObject;
            if (editable != null)
                editable.CancelEdit();
        }

        public void EndEdit()
        {
            var editable = Instance as IEditableObject;
            if (editable != null)
                editable.EndEdit();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj == this)
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals(Instance, ((ViewProxy)obj).Instance);
        }

        public override int GetHashCode()
        {
            return Instance.GetHashCode();
        }

        public override string ToString()
        {
            return Instance.ToString();
        }
    }

    [TypeDescriptionProvider(typeof(ProxyDescriptionProvider))]
    public sealed class ViewProxy<TViewModel> : ViewProxy
    {
        public static readonly ProxyTypeDescriptor TypeDescriptor = new ProxyTypeDescriptor(typeof(TViewModel));

        public ViewProxy(object instance)
            : base(instance)
        {
        }

        public override ProxyTypeDescriptor GetTypeDescriptor()
        {
            return TypeDescriptor;
        }
    }
}
