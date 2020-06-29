using Assets.Scripts;
using System;

namespace Criteria
{
    interface ICriterion
    {
        bool Eval(Query query);
    }

    #region Custom Criteria

    sealed class TargetNotSeen : ICriterion
    {
        public bool Eval(Query query) =>
            !query.Get<bool>("TargetSeen", StateSource.Event, out var seen) || !seen;
    }

    #endregion

    #region Generic Comparison Criteria

    abstract class GenericCriterion<T> : ICriterion
    {
        protected readonly string key;
        protected readonly T value;
        protected readonly StateSource source;

        protected GenericCriterion(string key, T value, StateSource source)
        {
            this.key = key;
            this.value = value;
            this.source = source;
        }

        public abstract bool Eval(Query query);
    }

    sealed class IsEqual<T> : GenericCriterion<T> where T : IEquatable<T>
    {
        public IsEqual(string key, T value, StateSource source) : base(key, value, source) { }

        public override bool Eval(Query query) => 
            query.Get<T>(key, source, out var current) && current.Equals(value);
    }

    sealed class IsNotEqual<T> : GenericCriterion<T> where T : IEquatable<T>
    {
        public IsNotEqual(string key, T value, StateSource source) : base(key, value, source) { }

        public override bool Eval(Query query) =>
            !query.Get<T>(key, source, out var current) || !current.Equals(value);
    }

    sealed class IsGreater<T> : GenericCriterion<T> where T : IComparable<T>
    {
        public IsGreater(string key, T value, StateSource source) : base(key, value, source) { }

        public override bool Eval(Query query) =>
            query.Get<T>(key, source, out var current) && current.CompareTo(value) > 0;
    }

    sealed class IsLess<T> : GenericCriterion<T> where T : IComparable<T>
    {
        public IsLess(string key, T value, StateSource source) : base(key, value, source) { }

        public override bool Eval(Query query) =>
            query.Get<T>(key, source, out var current) && current.CompareTo(value) < 0;
    }

    #endregion
}
