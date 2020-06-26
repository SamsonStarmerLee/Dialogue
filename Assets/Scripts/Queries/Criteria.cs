using Assets.Scripts;
using System;

namespace Criteria
{
    interface ICriterion
    {
        bool Eval(Query query);
    }

    #region Custom Criteria

    class ObjectNotSeen : ICriterion
    {
        public bool Eval(Query query)
        {
            return query.Get<bool>("ObjectSeen", StateSource.Event) == false;
        }
    }

    #endregion

    #region Generic Comparison Criteria

    class IsEqual<T> : ICriterion where T : IEquatable<T>
    {
        private string key;
        private T value;
        private StateSource source;

        public IsEqual(string key, T value, StateSource source)
        {
            this.key = key;
            this.value = value;
            this.source = source;
        }

        public bool Eval(Query query)
        {
            return query.Get<T>(key, source).Equals(value);
        }
    }

    class IsNotEqual<T> : ICriterion where T : IEquatable<T>
    {
        private string key;
        private T value;
        private StateSource source;

        public IsNotEqual(string key, T value, StateSource source)
        {
            this.key = key;
            this.value = value;
            this.source = source;
        }

        public bool Eval(Query query)
        {
            return !query.Get<T>(key, source).Equals(value);
        }
    }

    class IsGreater<T> : ICriterion where T : IComparable<T>
    {
        private string key;
        private T value;
        private StateSource source;

        public IsGreater(string key, T value, StateSource source)
        {
            this.key = key;
            this.value = value;
            this.source = source;
        }

        public bool Eval(Query query)
        {
            return query.Get<T>(key, source).CompareTo(value) > 0;
        }
    }

    class IsLess<T> : ICriterion where T : IComparable<T>
    {
        private string key;
        private T value;
        private StateSource source;

        public IsLess(string key, T value, StateSource source)
        {
            this.key = key;
            this.value = value;
            this.source = source;
        }

        public bool Eval(Query query)
        {
            return query.Get<T>(key, source).CompareTo(value) < 0;
        }
    }

    #endregion
}
