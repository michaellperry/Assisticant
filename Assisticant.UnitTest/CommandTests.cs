using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assisticant.Fields;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace Assisticant.UnitTest.WPF
{
    [TestClass]
    public class CommandTests
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
        public void CanMakeCommandWithNoParameter()
        {
            var ready = new Observable<bool>(false);
            var command = MakeCommand
                .When(() => ready)
                .Do(() => { });
            using (var counter = new CanExecuteCounter(command))
            {
                Process();
                counter.Count.Should().Be(1);
                ready.Value = true;
                Process();
                counter.Count.Should().Be(2);
            }
        }

        [TestMethod]
        public void CanMakeCommandWithConstantParameter()
        {
            var ready = new Observable<bool>(false);
            var command = MakeCommand
                .When(param => ready)
                .Do(param => { });
            using (var counter = new CanExecuteCounter(command))
            {
                Process();
                counter.Count.Should().Be(1);
                ready.Value = true;
                Process();
                counter.Count.Should().Be(2);
            }
        }

        private void Process()
        {
            while (_updateQueue.Any())
                _updateQueue.Dequeue()();
        }
    }
}
