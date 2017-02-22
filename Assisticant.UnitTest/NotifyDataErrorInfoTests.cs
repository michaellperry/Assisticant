using Assisticant.Fields;
using Assisticant.Validation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Assisticant.UnitTest.WPF
{
    [TestClass]
    public class NotifyDataErrorInfoTests
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

        class TestViewModel : IValidation
        {
            private Observable<string> _phoneNumber = new Observable<string>("(800)555-1212");
            private Observable<int> _age = new Observable<int>();
            private Observable<DateTime> _birth = new Observable<DateTime>();
            private Observable<DateTime?> _death = new Observable<DateTime?>();

            public string PhoneNumber
            {
                get { return _phoneNumber; }
                set { _phoneNumber.Value = value; }
            }

            public int Age
            {
                get { return _age; }
                set { _age.Value = value; }
            }

            public DateTime Birth
            {
                get { return _birth; }
                set { _birth.Value = value; }
            }

            public DateTime? Death
            {
                get { return _death; }
                set { _death.Value = value; }
            }

            public IValidationRules Rules => new ValidationRules()
                .For(() => PhoneNumber)
                    .Matches(@"^[0-9\-\(\)]*$")
                .For(() => PhoneNumber)  // I need to re-express the property.
                    .IsRequired()
                .ForInt(() => Age, r => r
                    .GreaterThanOrEqualTo(0))
                .ForInt(() => Age, r => r // Here, I'm just doing it to test that I can.
                    .LessThan(150))
                .For(() => Death)
                    .Where(v => v == null || v > Birth)
                    .WithMessage("Death date must be after birth date.");
        }

        [TestMethod]
        public void ImplementsINotifyDataErrorInfo()
        {
            TestViewModel viewModel = GivenViewModel();
            INotifyDataErrorInfo notify = GivenNotifyDataErrorInfo(viewModel);

            notify.Should().NotBeNull();
        }

        [TestMethod]
        public void InitiallyHasNoErrors()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);

            ShouldHaveNoError(notify);
        }

        [TestMethod]
        public void NotifiesWhenPropertyChanges()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            List<string> notified = new List<string>();
            EventHandler<DataErrorsChangedEventArgs> handler = (s, e) =>
                { notified.Add(e.PropertyName); };
            notify.ErrorsChanged += handler;
            viewModel.PhoneNumber = "abc";
            Process();
            notify.ErrorsChanged -= handler;

            string.Join(", ", notified).Should().Be("PhoneNumber");
        }

        [TestMethod]
        public void HasErrorWhenRegexNotMatched()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.PhoneNumber = "abc";

            ShouldHaveError(notify, "PhoneNumber", @"PhoneNumber is not valid");
        }

        [TestMethod]
        public void HasErrorWhenRegexPartiallyMatched()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.PhoneNumber = "abc123";

            ShouldHaveError(notify, "PhoneNumber", @"PhoneNumber is not valid");
        }

        [TestMethod]
        public void HasNoErrorWhenRegexIsMatched()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.PhoneNumber = "555-1212";

            ShouldHaveNoError(notify);
        }

        [TestMethod]
        public void HasErrorWhenTooSmall()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.Age = -1;

            ShouldHaveError(notify, "Age", "Age must be at least 0");
        }

        [TestMethod]
        public void HasErrorWhenTooBig()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.Age = 150;

            ShouldHaveError(notify, "Age", "Age must be less than 150");
        }

        [TestMethod]
        public void CanExpressCustomRule()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.Birth = DateTime.Parse("2010-01-01");
            viewModel.Death = DateTime.Parse("2009-01-01");

            ShouldHaveError(notify, "Death", "Death date must be after birth date.");
        }

        private void Process()
        {
            while (_updateQueue.Any())
                _updateQueue.Dequeue()();
        }

        private static TestViewModel GivenViewModel()
        {
            return new TestViewModel();
        }

        private static INotifyDataErrorInfo GivenNotifyDataErrorInfo(TestViewModel testViewModel)
        {
            var viewModel = ForView.Wrap(testViewModel);
            var notify = viewModel as INotifyDataErrorInfo;
            return notify;
        }

        private static void ShouldHaveNoError(INotifyDataErrorInfo notify)
        {
            notify.HasErrors.Should().BeFalse();
        }

        private static void ShouldHaveError(INotifyDataErrorInfo notify, string property, string expectedError)
        {
            notify.HasErrors.Should().BeTrue();
            string.Join(", ", notify.GetErrors(property).OfType<string>().ToArray())
                .Should().Be(expectedError);
        }
    }
}
