using Assets.Scripts;
using UnityEngine;

namespace Remember
{
    interface IRememberer
    {
        void Apply(Query query);
    }

    #region Custom Manipulations

    class RememberBarrel : IRememberer
    {
        public void Apply(Query query)
        {
            query.Increment("SeenBarrels", 1, StateSource.Memory);
            query.Set("TimeOfLastBarrelComment", Time.time, StateSource.Memory);

            // Tag object as seen.
            query
                .Get<GameObject>("Object", StateSource.Event)
                .SetMemory("ObjectSeen", true);
        }
    }

    #endregion

    #region Generic Manipulations

    class Set : IRememberer
    {
        private readonly string key;
        private readonly object value;
        private readonly StateSource source;

        public Set(string key, object value, StateSource source)
        {
            this.key = key;
            this.value = value;
            this.source = source;
        }

        public void Apply(Query query)
        {
            query.Set(key, value, source);
        }
    }

    #endregion

    #region Int Manipulations

    class AddInt : IRememberer
    {
        private readonly string key;
        private readonly int addition;
        private readonly StateSource source;

        public AddInt(string key, int addition, StateSource source)
        {
            this.key = key;
            this.addition = addition;
            this.source = source;
        }

        public void Apply(Query query)
        {
            var result = query.Get<int>(key, source) + addition;
            query.Set(key, result, source);
        }
    }

    class SubtractInt : IRememberer
    {
        private readonly string key;
        private readonly int subtraction;
        private readonly StateSource source;

        public SubtractInt(string key, int subtraction, StateSource source)
        {
            this.key = key;
            this.subtraction = subtraction;
            this.source = source;
        }

        public void Apply(Query query)
        {
            var result = query.Get<int>(key, source) - subtraction;
            query.Set(key, result, source);
        }
    }

    #endregion

    #region Float Manipulations

    class AddFloat : IRememberer
    {
        private readonly string key;
        private readonly float addition;
        private readonly StateSource source;

        public AddFloat(string key, float addition, StateSource source)
        {
            this.key = key;
            this.addition = addition;
            this.source = source;
        }

        public void Apply(Query query)
        {
            var result = query.Get<float>(key, source) + addition;
            query.Set(key, result, source);
        }
    }

    class SubtractFloat : IRememberer
    {
        private readonly string key;
        private readonly float subtraction;
        private readonly StateSource source;

        public SubtractFloat(string key, float subtraction, StateSource source)
        {
            this.key = key;
            this.subtraction = subtraction;
            this.source = source;
        }

        public void Apply(Query query)
        {
            var result = query.Get<float>(key, source) - subtraction;
            query.Set(key, result, source);
        }
    }

    class MultiplyFloat : IRememberer
    {
        private readonly string key;
        private readonly float multiplication;
        private readonly StateSource source;

        public MultiplyFloat(string key, float multiplication, StateSource source)
        {
            this.key = key;
            this.multiplication = multiplication;
            this.source = source;
        }

        public void Apply(Query query)
        {
            var result = query.Get<float>(key, source) * multiplication;
            query.Set(key, result, source);
        }
    }

    class DivideFloat : IRememberer
    {
        private readonly string key;
        private readonly float division;
        private readonly StateSource source;

        public DivideFloat(string key, float division, StateSource source)
        {
            this.key = key;
            this.division = division;
            this.source = source;
        }

        public void Apply(Query query)
        {
            var result = query.Get<float>(key, source) / division;
            query.Set(key, result, source);
        }
    }

    #endregion
}
