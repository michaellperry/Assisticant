using System.Collections.Generic;
using Assisticant.Collections;
using Assisticant.Fields;
using System;
using System.Linq;

namespace $rootnamespace$.Models
{
    public sealed class Document : IEquatable<Document>
    {
        private readonly Observable<string> _name = new Observable<string>();
        private readonly ObservableList<Item> _items = new ObservableList<Item>();

        public string Name
        {
            get { return _name; }
            set { _name.Value = value; }
        }

        public IEnumerable<Item> Items
        {
            get { return _items; }
        }

        public Item NewItem()
        {
            var item = new Item();
            _items.Add(item);
            return item;
        }

        public Item NewItem(string itemName)
        {
            var item = NewItem();
            item.Name = itemName;
            return item;
        }

        public void DeleteItem(Item item)
        {
            _items.Remove(item);
        }

        public bool CanMoveDown(Item item)
        {
            return IndexOf(item) < _items.Count - 1;
        }

        public bool CanMoveUp(Item item)
        {
            return IndexOf(item) > 0;
        }

        public void MoveDown(Item item)
        {
            var index = IndexOf(item);
            _items.RemoveAt(index);
            _items.Insert(index + 1, item);
        }

        public void MoveUp(Item item)
        {
            var index = IndexOf(item);
            _items.RemoveAt(index);
            _items.Insert(index - 1, item);
        }

        private int IndexOf(Item item)
        {
            return _items.IndexOf(item);
        }

        #region Equality
        public bool Equals(Document other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other as Document);
        }

        private bool IsEqualTo(Document other)
        {
            return Name.Equals(other.Name)
                && Items.SequenceEqual(other.Items);
        }

        public static bool operator ==(Document @this, Document other)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (ReferenceEquals(null, @this)) return false;

            return @this.Equals(other);
        }

        public static bool operator !=(Document @this, Document other)
        {
            return !(@this == other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) 
                    ^ (Items != null ? Items.GetHashCode() : 0);
            }
        }
        #endregion
    }
}
