using $rootnamespace$.Models;

namespace $rootnamespace$.ViewModels
{
    public sealed class ItemHeader
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

        public override bool Equals(object other)
        {
            if (other == this)
            {
                return true;
            }

            var that = other as ItemHeader;
            return that != null && _item.Equals(that._item);
        }

        public override int GetHashCode()
        {
            return _item.GetHashCode();
        }
    }
}
