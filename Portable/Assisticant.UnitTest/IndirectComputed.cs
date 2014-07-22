using Assisticant.Fields;

namespace Assisticant.UnitTest
{
	public class IndirectComputed
	{
		private DirectComputed _indermediateComputed;

        private Computed<int> _property;

        public IndirectComputed(DirectComputed indermediateComputed)
		{
			_indermediateComputed = indermediateComputed;
            _property = new Computed<int>(() => _indermediateComputed.ComputedProperty);
		}

		public int ComputedProperty
		{
			get { return _property; }
		}

		public bool IsUpToDate
		{
			get { return _property.IsUpToDate; }
		}
	}
}
