using System;
using Assisticant.Fields;

namespace $rootnamespace$.Models
{
    public sealed class Selection : IEquatable<Selection>
    {
        private readonly Observable<Item> _selectedItem = new Observable<Item>();

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem.Value = value; }
        }

        public bool IsItemSelected
        {
            get { return SelectedItem != null; }
        }

        #region Equality
        public bool Equals(Selection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other as Selection);
        }

        private bool IsEqualTo(Selection other)
        {
            return SelectedItem.Equals(other.SelectedItem);
        }

        public static bool operator ==(Selection @this, Selection other)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (ReferenceEquals(null, @this)) return false;

            return @this.Equals(other);
        }

        public static bool operator !=(Selection @this, Selection other)
        {
            return !(@this == other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((SelectedItem != null ? SelectedItem.GetHashCode() : 0));
            }
        }
        #endregion
    }
}
