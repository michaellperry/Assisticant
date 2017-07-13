using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assisticant.Validation
{
    class ExampleViewModel : IValidation
    {
        public int IntProperty { get; set; }
        public string StringProperty { get; set; }

        public IValidationRules Rules => Validator
            .For(() => IntProperty)
                .GreaterThan(0).WithMessage(() => "Must be positive")
                .LessThan(12).WithMessage(() => "But 12 is too much")
            .For(() => StringProperty)
            .Build();
    }
}
