using Assisticant.Fields;
using Assisticant.Validation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Linq;
using System;

namespace Assisticant.UnitTest.WPF
{
    [TestClass]
    public class NotifyDataErrorInfoTests
    {
        class TestViewModel : IValidation
        {
            private Observable<string> _phoneNumber = new Observable<string>();
            private Observable<int> _age = new Observable<int>();

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

            public ValidationRules Rules => new ValidationRules()
                .ForString(() => PhoneNumber, r => r
                    .Matches(@"^[0-9\-\(\)]*$"))
                .ForInt(() => Age, r => r
                    .GreaterThanOrEqualTo(0))
                .ForInt(() => Age, r => r
                    .LessThan(150));
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
