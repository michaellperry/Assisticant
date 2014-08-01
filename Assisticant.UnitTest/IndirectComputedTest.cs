#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Assisticant.UnitTest
{
    [TestClass]
	public class IndirectComputedTest
	{
		public TestContext TestContext { get; set; }

		private SourceData _source;
		private DirectComputed _intermediateComputed;
		private IndirectComputed _computed;

		[TestInitialize]
		public void Initialize()
		{
			_source = new SourceData();
			_intermediateComputed = new DirectComputed(_source);
			_computed = new IndirectComputed(_intermediateComputed);
		}

		[TestMethod]
		public void ComputedIsInitiallyOutOfDate()
		{
			Assert.IsFalse(_computed.IsUpToDate, "The dependent is initially up to date");
		}

		[TestMethod]
		public void ComputedRemainsOutOfDateOnChange()
		{
			_source.SourceProperty = 3;
			Assert.IsFalse(_computed.IsUpToDate, "The dependent is up to date after change");
		}

		[TestMethod]
		public void ComputedIsUpdatedOnGet()
		{
			int fetch = _computed.ComputedProperty;
			Assert.IsTrue(_computed.IsUpToDate, "The dependent has not been updated");
		}

		[TestMethod]
		public void ComputedIsUpdatedAfterChangeOnGet()
		{
			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;
			Assert.IsTrue(_computed.IsUpToDate, "The dependent has not been updated");
		}

		[TestMethod]
		public void ComputedGetsValueFromItsPrecedent()
		{
			_source.SourceProperty = 3;
			Assert.AreEqual(3, _computed.ComputedProperty);
		}

		[TestMethod]
		public void ComputedIsOutOfDateAgainAfterChange()
		{
			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;
			_source.SourceProperty = 4;
			Assert.IsFalse(_computed.IsUpToDate, "The dependent did not go out of date");
		}

		[TestMethod]
		public void ComputedIsUpdatedAgainAfterChange()
		{
			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;
			_source.SourceProperty = 4;
			fetch = _computed.ComputedProperty;
			Assert.IsTrue(_computed.IsUpToDate, "The dependent did not get udpated");
		}

		[TestMethod]
		public void ComputedGetsValueFromItsPrecedentAgainAfterChange()
		{
			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;
			_source.SourceProperty = 4;
			Assert.AreEqual(4, _computed.ComputedProperty);
		}

		[TestMethod]
		public void PrecedentIsOnlyAskedOnce()
		{
			int getCount = 0;
			_source.AfterGet += () => ++getCount;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;
			fetch = _computed.ComputedProperty;

			Assert.AreEqual(1, getCount);
		}

		[TestMethod]
		public void PrecedentIsAskedAgainAfterChange()
		{
			int getCount = 0;
			_source.AfterGet += () => ++getCount;

			_source.SourceProperty = 3;
			int fetch = _computed.ComputedProperty;
			fetch = _computed.ComputedProperty;
			_source.SourceProperty = 4;
			fetch = _computed.ComputedProperty;
			fetch = _computed.ComputedProperty;

			Assert.AreEqual(2, getCount);
		}
	}
}
