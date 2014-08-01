using System;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Assisticant.UnitTest
{
    public class NotifyingObservable : Observable
    {
        public event Action OnGainComputed;
        public event Action OnLoseComputed;

        protected override void GainDependent()
        {
            if (OnGainComputed != null)
                OnGainComputed();
        }

        protected override void LoseDependent()
        {
            if (OnLoseComputed != null)
                OnLoseComputed();
        }
    }

    [TestClass]
    public class NotificationTest
    {
        private bool _gained;
        private bool _lost;
        private NotifyingObservable _observable;
        private Computed _computed;
        private Computed _secondComputed;

        [TestInitialize]
        public void Initialize()
        {
            _gained = false;
            _observable = new NotifyingObservable();
            _observable.OnGainComputed += () => { _gained = true; };
            _observable.OnLoseComputed += () => { _lost = true; };
            _computed = new Computed(() => { _observable.OnGet(); });
            _secondComputed = new Computed(() => { _observable.OnGet(); });
        }

        [TestMethod]
        public void DoesNotGainComputedOnCreation()
        {
            Assert.IsFalse(_gained, "The observable should not have gained a dependent.");
        }

        [TestMethod]
        public void GainsComputedOnFirstUse()
        {
            _computed.OnGet();
            Assert.IsTrue(_gained, "The observable should have gained a dependent.");
        }

        [TestMethod]
        public void DoesNotGainComputedOnSecondUse()
        {
            _computed.OnGet();
            _gained = false;
            _secondComputed.OnGet();
            Assert.IsFalse(_gained, "The observable should not have gained a dependent.");
        }

        [TestMethod]
        public void DoesNotLoseComputedOnCreation()
        {
            Assert.IsFalse(_lost, "The observable should not have lost a dependent.");
        }

        [TestMethod]
        public void DoesNotLoseComputedOnFirstUse()
        {
            _computed.OnGet();
            Assert.IsFalse(_lost, "The observable should not have lost a dependent.");
        }

        [TestMethod]
        public void LosesComputedWhenChanging()
        {
            _computed.OnGet();
            _observable.OnSet();
            Assert.IsTrue(_lost, "The observable should have lost a dependent.");
        }
    }
}
