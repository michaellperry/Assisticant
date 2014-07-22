using Assisticant.Fields;

namespace Assisticant.UnitTest
{
	public class DirectComputed
	{
		private SourceData _source;

        private Computed<int> _property;

		public DirectComputed(SourceData source)
		{
			_source = source;
            _property = new Computed<int>(() => _source.SourceProperty);
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
