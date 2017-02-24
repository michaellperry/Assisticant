using System;
using System.Linq;
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
        private readonly Tuple<Func<T, bool>, Func<string>>[] _rules;

        public PropertyRuleset(Expression<Func<T>> propExpr)
        {
            PropExpr = propExpr;

            _rules = new Tuple<Func<T, bool>, Func<string>>[0];
        }

        private PropertyRuleset(PropertyRuleset<T> from, Func<T, bool> predicate, Func<string> messageFactory)
        {
            PropExpr = from.PropExpr;

            _rules = from._rules
                .Concat(new[] {Tuple.Create(predicate, messageFactory)})
                .ToArray();
        }

        public PropertyRuleset<T> AddRule(Func<T, bool> predicate, Func<string> messageFactory)
        {
            return new PropertyRuleset<T>(this, predicate, messageFactory);
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