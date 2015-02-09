using System;
using Assisticant.Fields;

namespace $rootnamespace$.Models
{
    public sealed class Item : IEquatable<Item>
    {
        private readonly Observable<string> _name = new Observable<string>();

        public string Name
        {
            get { return _name; }
            set { _name.Value = value; }
        }

        #region Equality
        public bool Equals(Item other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return IsEqualTo(other as Item);
        }

        private bool IsEqualTo(Item other)
        {
            return Name.Equals(other.Name);
        }

        public static bool operator ==(Item @this, Item other)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (ReferenceEquals(null, @this)) return false;

            return @this.Equals(other);
        }

        public static bool operator !=(Item @this, Item other)
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
