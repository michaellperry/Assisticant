using System;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Assisticant.UnitTest
{
	[TestClass]
	public class DirectConcurrencyTest
	{
		public TestContext TestContext { get; set; }

		private SourceData _source;
		private DirectComputed _computed;

		[TestInitialize]
		public void Initialize()
		{
			_source = new SourceData();
			_computed = new DirectComputed(_source);
		}

		[TestMethod]
		public void ComputedIsOutOfDateAfterConcurrentChange()
		{
			_source.AfterGet += () => _source.SourceProperty = 4;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;

			Assert.IsFalse(_computed.IsUpToDate, "The dependent is up to date after a concurrent change");
		}

		[TestMethod]
		public void ComputedHasOriginalValueAfterConcurrentChange()
		{
			_source.AfterGet += () => _source.SourceProperty = 4;

			_source.SourceProperty = 3;
			Assert.AreEqual(3, _computed.ComputedProperty);
		}

		[TestMethod]
		public void ComputedIsUpToDateAfterSecondGet()
		{
			Action concurrentChange = () => _source.SourceProperty = 4;
			_source.AfterGet += concurrentChange;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;

			_source.AfterGet -= concurrentChange;

			fetch = _computed.ComputedProperty;

			Assert.IsTrue(_computed.IsUpToDate, "The dependent is not up to date after the second get");
		}

		[TestMethod]
		public void ComputedHasModifiedValueAfterSecondGet()
		{
			Action concurrentChange = () => _source.SourceProperty = 4;
			_source.AfterGet += concurrentChange;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;

			_source.AfterGet -= concurrentChange;

			Assert.AreEqual(4, _computed.ComputedProperty);
		}

		[TestMethod]
		public void ComputedStillDependsUponPrecedent()
		{
			Action concurrentChange = () => _source.SourceProperty = 4;
			_source.AfterGet += concurrentChange;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;

			_source.AfterGet -= concurrentChange;

			fetch = _computed.ComputedProperty;
			_source.SourceProperty = 5;

			Assert.IsFalse(_computed.IsUpToDate, "The dependent no longer depends upon the precedent");
		}

		[TestMethod]
		public void ComputedGetsTheUltimateValue()
		{
			Action concurrentChange = () => _source.SourceProperty = 4;
			_source.AfterGet += concurrentChange;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;

			_source.AfterGet -= concurrentChange;

			fetch = _computed.ComputedProperty;
			_source.SourceProperty = 5;

			Assert.AreEqual(5, _computed.ComputedProperty);
		}
	}
}
