using System;
using System.Collections.Generic;

namespace Assisticant.Validation
{
    public static class NumericPropertyValidationContextExtentions
    {
        public static OptionalMessagePropertyValidationContext<T> GreaterThan<T>(
            this PropertyValidationContextBase<T> context,
            T lowerBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context.PropertyName;

            return context.BeginPredicate(
                x => comparer.Compare(x, lowerBound) > 0,
                () => $"{name} must be greater than {lowerBound}");
        }

        public static OptionalMessagePropertyValidationContext<T> GreaterThanOrEqualTo<T>(
            this PropertyValidationContextBase<T> context,
            T lowerBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context.PropertyName;

            return context.BeginPredicate(
                x => comparer.Compare(x, lowerBound) >= 0,
                () => $"{name} must be at least {lowerBound}");
        }

        public static OptionalMessagePropertyValidationContext<T> LessThan<T>(
            this PropertyValidationContextBase<T> context,
            T upperBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context.PropertyName;

            return context.BeginPredicate(
                x => comparer.Compare(x, upperBound) < 0,
                () => $"{name} must be less than {upperBound}");
        }

        public static OptionalMessagePropertyValidationContext<T> LessThanOrEqualTo<T>(
            this PropertyValidationContextBase<T> context,
            T upperBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context.PropertyName;

            return context.BeginPredicate(
                x => comparer.Compare(x, upperBound) > 0,
                () => $"{name} must be no more than {upperBound}");
        }

        private static Comparer<T> DefaultIfNull<T>(Comparer<T> comparer)
            where T : IComparable<T>, IComparable
        {
            return comparer != null ? comparer : Comparer<T>.Default;
        }

        internal static OptionalMessagePropertyValidationContext<T> BeginPredicate<T>(
            this PropertyValidationContextBase<T> context,
            Func<T, bool> predicate,
            Func<string> messageFactory
        )
        {
            return new OptionalMessagePropertyValidationContext<T>(
                context._rulesets,
                context.FinalizeRuleset(),
                predicate,
                messageFactory
                );
        }
    }
}