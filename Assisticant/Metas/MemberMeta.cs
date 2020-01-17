using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public abstract class MemberMeta
    {
        public readonly TypeMeta DeclaringType;
        public readonly string Name;
        public readonly Type MemberType;
        public readonly bool IsViewModel;
        public readonly bool IsCollection;
		public readonly bool IsList;
		public readonly bool IsObservableCollection;

        public virtual bool CanRead { get { return true; } }
        public virtual bool CanWrite { get { return true; } }

        public virtual IEnumerable<MemberMeta> EarlierMembers => Enumerable.Empty<MemberMeta>();


		public MemberMeta(TypeMeta owner, string name, Type type)
        {
            DeclaringType = owner;
            Name = name;
            MemberType = type;
            IsViewModel = ViewModelTypes.IsViewModel(type);
            IsCollection = typeof(IEnumerable).IsAssignableFromPortable(MemberType) && MemberType != typeof(string);
            IsObservableCollection = typeof(INotifyCollectionChanged).IsAssignableFromPortable(type) ||
                IsBindingList(type);
			IsList = typeof(IList).IsAssignableFromPortable(type) ||
				IsIList(type);
        }

		public abstract void SetValue(object instance, object value);
        public abstract object GetValue(object instance);

        public override string ToString()
        {
            return String.Format("{0}.{1}", DeclaringType.Type.Name, Name);
        }

        private bool IsBindingList(Type type)
        {
            const string bindingList = "System.ComponentModel.IBindingList";
            return type.FullName == bindingList ||
                type.GetInterfacesPortable().Any(i => i.FullName == bindingList);
        }

		private bool IsIList(Type type)
		{
			const string iList = "System.Collections.Generic.IList";
			return type.FullName.Contains(iList) ||
				type.GetInterfacesPortable().Any(i => i.FullName.Contains(iList));
		}
	}
}
