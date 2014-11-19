using Assisticant.Fields;

namespace $rootnamespace$.Models
{
    public sealed class Item
    {
        private readonly Observable<string> _name = new Observable<string>();

        public string Name
        {
            get { return _name; }
            set { _name.Value = value; }
        }
    }
}
