using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Metas
{
    public class CollectionSlot : MemberSlot
    {
        List<object> _sourceCollection;
		ObservableCollection<object> _collection = new ObservableCollection<object>();
        Func<IEnumerable, IEnumerable> _translateIncomingList;

        public CollectionSlot(ViewProxy proxy, MemberMeta member)
            : base(proxy, member)
		{
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
            // own, if it happens to be ObservableCollection<object>.
            _collection = value as ObservableCollection<object>;
        }

        public override object GetValue()
        {
            UpdateNow();
            return _collection;
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

            // Delay the update to the observable collection so that we don't record dependencies on
            // properties used in the items template. XAML will invoke the item template synchronously
            // as we add items to the observable collection, thus causing other view model property
            // getters to fire.
        }

        protected override void PublishChanges()
        {
            if (_sourceCollection == null)
            {
                if (_collection != null)
                {
                    _collection = null;
                    FirePropertyChanged();
                }
                return;
            }

            if (_collection == null)
            {
                _collection = new ObservableCollection<object>();
                FirePropertyChanged();
            }

            // Create a list of new items.
            List<CollectionItem> items = new List<CollectionItem>();

            // Dump all previous items into a recycle bin.
            using (RecycleBin<CollectionItem> bin = new RecycleBin<CollectionItem>())
            {
                foreach (object oldItem in _collection)
                    bin.AddObject(new CollectionItem(_collection, oldItem, true));
                // Add new objects to the list.
                if (_sourceCollection != null)
                    foreach (object obj in _sourceCollection)
                        items.Add(bin.Extract(new CollectionItem(_collection, WrapValue(obj), false)));
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
    }
}
