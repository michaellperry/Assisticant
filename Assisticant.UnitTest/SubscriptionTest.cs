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
        private static Queue<Action> _updateQueue;

        [TestInitialize]
        public void Initialize()
        {
            if (_updateQueue == null)
            {
                _updateQueue = new Queue<Action>();
                UpdateScheduler.Initialize(a => _updateQueue.Enqueue(a));
            }
            _updateQueue.Clear();
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

        [TestMethod]
        public void OuterComputedDoesNotTakeADependency()
        {
            Observable<int> source = new Observable<int>();
            Computed<Counter> outer = new Computed<Counter>(() => new Counter(source));

            int created = 0;
            Counter counter = null;
            outer.Subscribe(c =>
            {
                counter = c;
                created++;
            });

            Process();
            created.Should().Be(1);
            counter.Count.Should().Be(1);

            source.Value = 42;
            Process();
            created.Should().Be(1);
            counter.Count.Should().Be(2);
        }

        [TestMethod]
        public void SubscriberCanReceivePriorValue()
        {
            Observable<int> source = new Observable<int>();
            Computed<int> target = new Computed<int>(() => source);

            int newValue = 999, oldValue = 999;
            source.Value = 3;
            target.Subscribe((n, o) => { newValue = n; oldValue = o; });

            Process();
            newValue.Should().Be(3);
            oldValue.Should().Be(0);

            source.Value = 7;
            Process();
            newValue.Should().Be(7);
            oldValue.Should().Be(3);
        }

        private void Process()
        {
            while (_updateQueue.Any())
                _updateQueue.Dequeue()();
        }

        private class Counter
        {
            private Computed<int> _target;
            private int _count;

            public Counter(Observable<int> source)
            {
                _target = new Computed<int>(() => source.Value);
                _target.Subscribe(i => _count++);
            }

            public int Count
            {
                get { return _count; }
            }
        }
    }
}
