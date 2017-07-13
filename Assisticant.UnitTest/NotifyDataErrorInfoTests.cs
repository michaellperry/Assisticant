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

        class Person { }

        class TestViewModel : IValidation
        {
            private Observable<string> _phoneNumber = new Observable<string>("(123)456-7890");
            private Observable<int> _age = new Observable<int>();
            private Observable<DateTime> _birth = new Observable<DateTime>();
            private Observable<DateTime?> _death = new Observable<DateTime?>();
            private Observable<Person> _friend = new Observable<Person>(new Person());

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

            public Person Friend
            {
                get { return _friend; }
                set { _friend.Value = value; }
            }

            public IValidationRules Rules => 
                Validator
                .For(() => PhoneNumber)
                    .NotNullOrWhitespace()
                        .WithMessage(() => "Phone number is required")
                    .Matches(@"^[0-9\-\(\)]*$")
                        .WithMessage(() => "Phone number may contain only parentheses, dashes, and digits.")
                    .MaxLength(20)
                .For(() => Age)
                    .GreaterThanOrEqualTo(0)
                    .LessThan(150)
                .For(() => Death)
                    .Where(v => v == null || v > Birth)
                    .WithMessage(() => "Death date must be after birth date.")
                .For(() => Friend)
                    .NotNull()
                .Build();
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

        private void ShouldHaveErrorWhen(Action<TestViewModel> condition, string property, string expectedError)
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            condition(viewModel);

            ShouldHaveError(notify, property, expectedError);
        }

        [TestMethod]
        public void HasErrorOnStringRequired()
        {
            ShouldHaveErrorWhen(
                vm => vm.PhoneNumber = null,
                "PhoneNumber",
                "Phone number is required");
        }

        [TestMethod]
        public void HasErrorOnStringMaxLengthExceeded()
        {
            ShouldHaveErrorWhen(
                vm => vm.PhoneNumber = "012345678901234567890",
                "PhoneNumber",
                "PhoneNumber may be at most 20 characters long.");
        }

        [TestMethod]
        public void HasErrorWhenRegexNotMatched()
        {
            ShouldHaveErrorWhen(
                vm => vm.PhoneNumber = "abc",
                "PhoneNumber",
                "Phone number may contain only parentheses, dashes, and digits.");
        }

        [TestMethod]
        public void HasErrorWhenRegexPartiallyMatched()
        {
            ShouldHaveErrorWhen(
                vm => vm.PhoneNumber = "abc123",
                "PhoneNumber",
                "Phone number may contain only parentheses, dashes, and digits.");
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
            ShouldHaveErrorWhen(
                vm => vm.Age = -1,
                "Age",
                "Age must be at least 0");
        }

        [TestMethod]
        public void HasErrorWhenTooBig()
        {
            ShouldHaveErrorWhen(
                vm => vm.Age = 150,
                "Age",
                "Age must be less than 150");
        }

        [TestMethod]
        public void CanExpressCustomRule()
        {
            ShouldHaveErrorWhen(
                vm =>
                {
                    vm.Birth = DateTime.Parse("2010-01-01");
                    vm.Death = DateTime.Parse("2009-01-01");
                },
                "Death",
                "Death date must be after birth date.");
        }

        [TestMethod]
        public void HasErrorWhenNull()
        {
            ShouldHaveErrorWhen(
                vm => vm.Friend = null,
                "Friend",
                "Friend must not be null.");
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
