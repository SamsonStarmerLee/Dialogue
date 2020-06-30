using Assets.Scripts;
using Queries;
using System.Security.Permissions;

namespace Queries
{
    abstract class Rememberer
    {
        public Rememberer(string key, StateSource source, float value)
        {
            this.key = key;
            this.source = source;
            this.value = value;
        }

        protected readonly string key;
        protected readonly StateSource source;
        protected readonly float value;

        public abstract void Remember(Query query);
    }

    class Set : Rememberer
    {
        public Set(string key, StateSource source, float value) :
            base(key, source, value) { }

        public override void Remember(Query query)
        {
            query.Set(key, value, source);
        }
    }

    class Add : Rememberer
    {
        public Add(string key, StateSource source, float value) :
            base(key, source, value)
        { }

        public override void Remember(Query query)
        {
            query.Get(key, source, out float current);
            query.Set(key, current + value, source);
        }
    }

    class Subtract : Rememberer
    {
        public Subtract(string key, StateSource source, float value) :
            base(key, source, value)
        { }

        public override void Remember(Query query)
        {
            query.Get(key, source, out float current);
            query.Set(key, current - value, source);
        }
    }

    class Multiply : Rememberer
    {
        public Multiply(string key, StateSource source, float value) :
            base(key, source, value)
        { }

        public override void Remember(Query query)
        {
            query.Get(key, source, out float current);
            query.Set(key, current * value, source);
        }
    }

    class Divide : Rememberer
    {
        public Divide(string key, StateSource source, float value) :
            base(key, source, value)
        { }

        public override void Remember(Query query)
        {
            query.Get(key, source, out float current);
            query.Set(key, current / value, source);
        }
    }
}

