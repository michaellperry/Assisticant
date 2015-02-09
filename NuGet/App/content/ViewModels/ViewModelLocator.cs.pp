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
                return ViewModel(() => _selection.IsItemSelected
                    ? new ItemViewModel(_selection.SelectedItem)
                    : null);
            }
        }

        private static Document LoadDocument()
        {
            // TODO: Load your document here.
            var document = new Document();

            document.NewItem("One");
            document.NewItem("Two");
            document.NewItem("Three");

            return document;
        }

        private static Document LoadDesignModeDocument()
        {
            var document = new Document();

            document.NewItem("Design");
            document.NewItem("Mode");
            document.NewItem("Data");

            return document;
        }
    }
}
