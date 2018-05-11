using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Assisticant.Metas
{
    public class BindingListSlot : MemberSlot
    {
        public static bool AppliesTo(MemberMeta member)
        {
            return GetNewItemMethod(member) != null;
        }

        MethodInfo _newItemMethod;
        List<object> _sourceCollection;
        BindingList<object> _bindingList = new BindingList<object>();
        Func<IEnumerable, IEnumerable> _translateIncomingList;

        public BindingListSlot(ViewProxy proxy, MemberMeta member)
            : base(proxy, member)
        {
            _newItemMethod = GetNewItemMethod(member);

            _bindingList.AllowNew = true;
            _bindingList.AllowEdit = true;
            _bindingList.AllowRemove = true;
            _bindingList.AddingNew += BindingList_AddingNew;
        }

        public override void SetValue(object value)
        {
            // Use reflection to convert the collection to the ViewModel type
            // (which must be compatible with List<T>, e.g. IEnumerable<T> or IList)
            if (_translateIncomingList == null)
            {
                Type propType = Member.MemberType;
                Type elemType = (propType.GetInterfacesPortable().Concat(new Type[] { propType })
                    .FirstOrDefault(i => i.IsGenericTypePortable() && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ?? typeof(IEnumerable<object>))
                    .GetGenericArgumentsPortable().First();
                MethodInfo mi = GetType().GetMethodPortable("TranslateIncomingList").MakeGenericMethod(new Type[] { elemType });
                _translateIncomingList = (Func<IEnumerable, IEnumerable>)mi.CreateDelegatePortable(typeof(Func<IEnumerable, IEnumerable>), this);
            }
            value = _translateIncomingList((IEnumerable)value);
            Member.SetValue(Instance, value);

            // If the UI object switches to a new collection, we can expect it to
            // cancel its subscription to _collection.CollectionChanged and subscribe
            // to the new collection instead. So let's adopt the collection as our
            // own, if it happens to be BindingList<object>.
            _bindingList = value as BindingList<object>;
        }

        public override object GetValue()
        {
            UpdateNow();
            return _bindingList;
        }

        internal override void UpdateValue()
        {
            // Get the source collection from the wrapped object.
            IEnumerable source = Member.GetValue(Instance) as IEnumerable;
            if (source == null)
            {
                _sourceCollection = null;
                return;
            }

            _sourceCollection = source.OfType<object>().ToList();

            // Delay the update to the binding list so that we don't record dependencies on
            // properties used in the items template. XAML will invoke the item template synchronously
            // as we add items to the binding list, thus causing other view model property
            // getters to fire.
        }

        protected override void PublishChanges()
        {
            if (_sourceCollection == null)
            {
                if (_bindingList != null)
                {
                    _bindingList = null;
                    FirePropertyChanged();
                }
                return;
            }

            if (_bindingList == null)
            {
                _bindingList = new BindingList<object>();
                FirePropertyChanged();
            }

            // Create a list of new items.
            List<CollectionItem> items = new List<CollectionItem>();

            // Dump all previous items into a recycle bin.
            using (RecycleBin<CollectionItem> bin = new RecycleBin<CollectionItem>())
            {
                foreach (object oldItem in _bindingList)
                    bin.AddObject(new CollectionItem(_bindingList, oldItem, true));
                // Add new objects to the list.
                if (_sourceCollection != null)
                    foreach (object obj in _sourceCollection)
                        items.Add(bin.Extract(new CollectionItem(_bindingList, WrapValue(obj), false)));
                // All deleted items are removed from the collection at this point.
            }
            // Ensure that all items are added to the list.
            int index = 0;
            foreach (CollectionItem item in items)
            {
                item.EnsureInCollection(index);
                ++index;
            }
        }

        IEnumerable TranslateIncomingList<T>(IEnumerable list)
        {
            var translated = new List<T>();
            foreach (object elem in list)
                translated.Add((T)UnwrapValue(elem));
            return translated;
        }

        private void BindingList_AddingNew(object sender, AddingNewEventArgs e)
        {
            var item = _newItemMethod.Invoke(Instance, new object[0]);
            e.NewObject = WrapValue(item);
        }

        private static MethodInfo GetNewItemMethod(MemberMeta member)
        {
            var method = member.DeclaringType.Type.GetMethodPortable($"NewItemIn{member.Name}");
            if (method.GetParameters().Length == 0)
            {
                return method;
            }
            else
            {
                return null;
            }
        }

        class CollectionItem : IDisposable
        {
            private readonly BindingList<object> _bindingList;
            private readonly object _wrappedItem;
            private readonly bool _existingItem;

            public CollectionItem(BindingList<object> bindingList, object wrappedItem, bool existingItem)
            {
                _bindingList = bindingList;
                _wrappedItem = wrappedItem;
                _existingItem = existingItem;
            }

            public void Dispose()
            {
                if (_existingItem)
                    _bindingList.Remove(_wrappedItem);
            }

            public void EnsureInCollection(int index)
            {
                if (!_existingItem)
                {
                    // Insert the item into the correct position.
                    _bindingList.Insert(index, _wrappedItem);
                }
                else if (!object.Equals(_bindingList[index], _wrappedItem))
                {
                    // Remove the item from the old position.
                    _bindingList.Remove(_wrappedItem);

                    // Insert the item in the correct position.
                    _bindingList.Insert(index, _wrappedItem);
                }
            }

            public override int GetHashCode()
            {
                return _wrappedItem.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (obj == this)
                    return true;
                if (!(obj is CollectionItem))
                    return false;
                CollectionItem that = (CollectionItem)obj;
                return Object.Equals(
                    this._wrappedItem,
                    that._wrappedItem);
            }
        }
    }
}
