using System.Collections.Generic;

namespace Assisticant.UnitTest.CollectionData
{
    public class SourceCollection
    {
        private List<int> _numbers = new List<int>();
        private Observable _indNumbers = new Observable();

        public void Insert(int number)
        {
            _indNumbers.OnSet();
            _numbers.Add(number);
        }

        public IEnumerable<int> Numbers
        {
            get { _indNumbers.OnGet(); return _numbers; }
        }
    }
}
