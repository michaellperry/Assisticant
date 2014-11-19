using Assisticant.Fields;

namespace $rootnamespace$.Models
{
    public sealed class Selection
    {
        private readonly Observable<Item> _selectedItem = new Observable<Item>();

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem.Value = value; }
        }
    }
}
