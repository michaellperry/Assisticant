using Assisticant.Fields;
using Assisticant.Validation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace Assisticant.UnitTest.WPF
{
    [TestClass]
    public class NotifyDataErrorInfoTests
    {
        class TestViewModel : IValidation
        {
            private Observable<string> _phoneNumber = new Observable<string>();

            public string PhoneNumber
            {
                get { return _phoneNumber; }
                set { _phoneNumber.Value = value; }
            }

            public ValidationRules Rules => new ValidationRules()
                .For(() => PhoneNumber, r => r
                    .Matches(@"[0-9\-\(\)]*"));
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

            notify.HasErrors.Should().BeFalse();
        }

        [TestMethod]
        public void HasErrorWhenRegexNotMatched()
        {
            var viewModel = GivenViewModel();
            var notify = GivenNotifyDataErrorInfo(viewModel);
            viewModel.PhoneNumber = "abc";

            notify.HasErrors.Should().BeTrue();
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
    }
}
