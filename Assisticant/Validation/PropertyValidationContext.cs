using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    public sealed class PropertyValidationContext<T> : PropertyValidationContextBase<T>
    {
        internal PropertyValidationContext(IEnumerable<PropertyRuleset> rulesets, Expression<Func<T>> propExpression)
            : base(rulesets, propExpression)
        {
        }

        internal PropertyValidationContext(IEnumerable<PropertyRuleset> rulesets, PropertyRuleset<T> currentRuleset)
            : base(rulesets, currentRuleset)
        {
        }
    }
}