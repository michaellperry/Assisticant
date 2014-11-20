using System;
using $rootnamespace$.Models;

namespace $rootnamespace$.ViewModels
{
    public sealed class ItemViewModel : IEquatable<ItemViewModel>
    {
        private readonly Item _item;

        public ItemViewModel(Item item)
        {
            _item = item;
        }

        public string Name
        {
            get { return _item.Name; }
            set { _item.Name = value; }
        }

        #region Equality
        public bool Equals(ItemViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other as ItemViewModel);
        }

        private bool IsEqualTo(ItemViewModel other)
        {
            return Name.Equals(other.Name);
        }

        public static bool operator ==(ItemViewModel @this, ItemViewModel other)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (ReferenceEquals(null, @this)) return false;

            return @this.Equals(other);
        }

        public static bool operator !=(ItemViewModel @this, ItemViewModel other)
        {
            return !(@this == other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0));
            }
        }
        #endregion
    }
}
