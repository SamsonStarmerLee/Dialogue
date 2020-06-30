using Assets.Scripts;
using System;

namespace Queries
{
    class Criterion
    {
        public Criterion(string key, StateSource source, float a, float b)
        {
            Key = key;
            Source = source;
            this.a = a;
            this.b = b;
        }

        private readonly float a, b;
    
        public string Key { get; }

        public StateSource Source { get; }

        public bool Compare(float x)
        {
            return x >= a &&
                   x <= b;
        }
    }
}
