using $rootnamespace$.Models;

namespace $rootnamespace$.ViewModels
{
    public sealed class ItemViewModel
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

        public override bool Equals(object other)
        {
            if (other == this)
            {
                return true;
            }

            var that = other as ItemViewModel;
            return that != null && _item.Equals(that._item);
        }

        public override int GetHashCode()
        {
            return _item.GetHashCode();
        }
    }
}
