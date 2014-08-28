using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assisticant.Fields;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace Assisticant.UnitTest
{
    [TestClass]
    public class SubscriptionTest
    {
        private Queue<Action> _updateQueue;

        [TestInitialize]
        public void Initialize()
        {
            _updateQueue = new Queue<Action>();
            UpdateScheduler.Initialize(a => _updateQueue.Enqueue(a));
        }

        [TestMethod]
        public void CanSubscribeToAComputed()
        {
            Observable<int> source = new Observable<int>();
            Computed<int> target = new Computed<int>(() => source);

            int count = 0;
            target.Subscribe(i => count++);

            Process();
            count.Should().Be(1);
            source.Value = 42;
            Process();
            count.Should().Be(2);
        }

        [TestMethod]
        public void CanUnsubscribeFromAComputed()
        {
            Observable<int> source = new Observable<int>();
            Computed<int> target = new Computed<int>(() => source);

            int count = 0;
            var subscription = target.Subscribe(i => count++);

            Process();
            count.Should().Be(1);
            source.Value = 42;
            Process();
            count.Should().Be(2);

            subscription.Unsubscribe();
            source.Value = 24;
            Process();
            count.Should().Be(2);
        }

        private void Process()
        {
            while (_updateQueue.Any())
                _updateQueue.Dequeue()();
        }
    }
}
