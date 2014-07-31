using System.Collections.Generic;
using System.Linq;

namespace Assisticant.UnitTest.CollectionData
{
    public class TargetCollection
    {
        private SourceCollection _source;
        private List<int> _results = new List<int>();
        private Computed _depResults;

        public TargetCollection(SourceCollection source)
        {
            _source = source;
            _depResults = new Computed(UpdateResults);
        }

        public IEnumerable<int> Results
        {
            get { _depResults.OnGet(); return _results; }
        }

        private void UpdateResults()
        {
            _results = _source.Numbers.Select(number => number + 1).ToList();
        }
    }
}
