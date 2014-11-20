using System;
using $rootnamespace$.Models;

namespace $rootnamespace$.ViewModels
{
    public sealed class ItemHeader : IEquatable<ItemHeader>
    {
        private readonly Item _item;

        public ItemHeader(Item item)
        {
            _item = item;
        }

        public Item Item
        {
            get { return _item; }
        }

        public string Name
        {
            get { return _item.Name ?? "<New Item>"; }
        }

        #region Equality
        public bool Equals(ItemHeader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other as ItemHeader);
        }

        private bool IsEqualTo(ItemHeader other)
        {
            return Item.Equals(other.Item);
        }

        public static bool operator ==(ItemHeader @this, ItemHeader other)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (ReferenceEquals(null, @this)) return false;

            return @this.Equals(other);
        }

        public static bool operator !=(ItemHeader @this, ItemHeader other)
        {
            return !(@this == other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Item != null ? Item.GetHashCode() : 0));
            }
        }
        #endregion
    }
}
