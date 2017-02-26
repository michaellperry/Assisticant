using System;
using System.Collections.Generic;

namespace Assisticant.Validation
{
    public static class NumericPropertyValidationContextExtentions
    {
        public static PropertyPredicateContext<T> GreaterThan<T>(
            this PropertyValidationContext<T> context,
            T lowerBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context._currentProperty.GetPropertyName();

            return context.BeginPredicate(
                x => comparer.Compare(x, lowerBound) > 0,
                () => $"{name} must be greater than {lowerBound}");
        }

        public static PropertyPredicateContext<T> GreaterThanOrEqualTo<T>(
            this PropertyValidationContext<T> context,
            T lowerBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context._currentProperty.GetPropertyName();

            return context.BeginPredicate(
                x => comparer.Compare(x, lowerBound) >= 0,
                () => $"{name} must be at least {lowerBound}");
        }

        public static PropertyPredicateContext<T> LessThan<T>(
            this PropertyValidationContext<T> context,
            T upperBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context._currentProperty.GetPropertyName();

            return context.BeginPredicate(
                x => comparer.Compare(x, upperBound) < 0,
                () => $"{name} must be less than {upperBound}");
        }

        public static PropertyPredicateContext<T> LessThanOrEqualTo<T>(
            this PropertyValidationContext<T> context,
            T upperBound,
            Comparer<T> comparer = null)
            where T : IComparable<T>, IComparable
        {
            comparer = DefaultIfNull(comparer);
            var name = context._currentProperty.GetPropertyName();

            return context.BeginPredicate(
                x => comparer.Compare(x, upperBound) > 0,
                () => $"{name} must be no more than {upperBound}");
        }

        private static Comparer<T> DefaultIfNull<T>(Comparer<T> comparer)
            where T : IComparable<T>, IComparable
        {
            return comparer != null ? comparer : Comparer<T>.Default;
        }
    }
}