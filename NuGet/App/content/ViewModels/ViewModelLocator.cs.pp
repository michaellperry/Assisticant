using Assisticant;
using $rootnamespace$.Models;

namespace $rootnamespace$.ViewModels
{
    public sealed class ViewModelLocator : ViewModelLocatorBase
    {
        private readonly Document _document;
        private readonly Selection _selection;

        public ViewModelLocator()
        {
            _document = DesignMode 
                ? LoadDesignModeDocument() 
                : LoadDocument();

            _selection = new Selection();
        }

        public object Main
        {
            get { return ViewModel(() => new MainViewModel(_document, _selection)); }
        }

        public object Item
        {
            get
            {
                return ViewModel(() => _selection.SelectedItem == null
                    ? null
                    : new ItemViewModel(_selection.SelectedItem));
            }
        }

        private static Document LoadDocument()
        {
            // TODO: Load your document here.
            var document = new Document();
            var one = document.NewItem();
            one.Name = "One";
            var two = document.NewItem();
            two.Name = "Two";
            var three = document.NewItem();
            three.Name = "Three";
            return document;
        }

        private static Document LoadDesignModeDocument()
        {
            var document = new Document();
            var one = document.NewItem();
            one.Name = "Design";
            var two = document.NewItem();
            two.Name = "Mode";
            var three = document.NewItem();
            three.Name = "Data";
            return document;
        }
    }
}
