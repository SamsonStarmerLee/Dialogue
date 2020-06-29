using Assets.Scripts;
using System;

namespace Queries
{
    delegate bool Criterion(Query query);

    static class Criteria
    {
        #region Generic Comparison Criteria

        public static bool Equal<T>(Query query, string key, T value, StateSource source) where T : IEquatable<T>
        {
            return 
                query.Get<T>(key, source, out var current) && 
                current.Equals(value);
        }

        public static bool GreaterThan<T>(Query query, string key, T value, StateSource source) where T : IComparable<T>
        {
            return 
                query.Get<T>(key, source, out var current) && 
                current.CompareTo(value) > 0;
        }

        public static bool LessThan<T>(Query query, string key, T value, StateSource source) where T : IComparable<T>
        {
            return
                query.Get<T>(key, source, out var current) &&
                current.CompareTo(value) < 0;
        }

        #endregion
    }
}
