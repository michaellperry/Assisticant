using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    [DebuggerDisplay("ForView.Wrap({Instance})")]
    public abstract class ViewProxy : INotifyPropertyChanged, IEditableObject
    {
        public readonly object Instance;
        readonly MemberSlot[] _slots;
        private int _notifyCount = 0;
        private HashSet<MemberSlot> _modifiedWhileNotifying = new HashSet<MemberSlot>();

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract ViewProxy WrapObject(object value);

        protected ViewProxy(object instance, TypeMeta type)
        {
            Instance = instance;
            _slots = (from member in type.Members
                      select MemberSlot.Create(this, member)).ToArray();
        }

        public void FirePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public MemberSlot LookupSlot(MemberMeta member)
        {
            return _slots.FirstOrDefault(s => s.Member == member);
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

        internal void Notify(Action publishChanges)
        {
            _notifyCount++;
            try
            {
                publishChanges();
            }
            finally
            {
                _notifyCount--;
                if (_notifyCount == 0)
                {
                    foreach (var member in _modifiedWhileNotifying)
                    {
                        member.PublishChanges();
                    }
                    _modifiedWhileNotifying.Clear();
                }
            }
        }

        internal bool IsNotifying => _notifyCount > 0;

        internal void WasModified(MemberSlot member)
        {
            _modifiedWhileNotifying.Add(member);
        }
    }
}
