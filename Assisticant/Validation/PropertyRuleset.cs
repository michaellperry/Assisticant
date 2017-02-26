using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Assisticant.Validation
{
    internal abstract class PropertyRuleset
    {
        public abstract void AddTo(ValidationRules rules);
    }

    internal sealed class PropertyRuleset<T> : PropertyRuleset
    {
        public Expression<Func<T>> PropExpr { get; }
        private readonly IEnumerable<Tuple<Func<T, bool>, Func<string>>> _rules;

        public PropertyRuleset(Expression<Func<T>> propExpr, IEnumerable<Tuple<Func<T, bool>, Func<string>>> rules)
        {
            PropExpr = propExpr;

            _rules = rules;
        }

        public override void AddTo(ValidationRules rules)
        {
            var validator = rules.ValidatorForProperty(PropExpr);

            foreach (var rule in _rules)
            {
                validator.AddRule(x => rule.Item1((T)x), rule.Item2);
            }
        }
    }
}