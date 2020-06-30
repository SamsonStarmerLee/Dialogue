namespace Queries
{
    delegate void Rememberer(Query query);

    static class Rememberers
    {
        #region Generic Maniupulations

        public static void Set(Query query, string key, object value, StateSource source)
        {
            query.Set(key, value, source);
        }

        #endregion

        #region Int Manipulations

        public static void AddInt(Query query, string key, int addition, StateSource source)
        {
            query.Get<int>(key, source, out var current);
            query.Set(key, current + addition, source);
        }

        public static void SubtractInt(Query query, string key, int subtraction, StateSource source)
        {
            query.Get<int>(key, source, out var current);
            query.Set(key, current - subtraction, source);
        }

        #endregion

        #region Float Manipulations

        public static void AddFloat(Query query, string key, float addition, StateSource source)
        {
            query.Get<float>(key, source, out var current);
            query.Set(key, current + addition, source);
        }

        public static void SubtractFloat(Query query, string key, float subtraction, StateSource source)
        {
            query.Get<float>(key, source, out var current);
            query.Set(key, current - subtraction, source);
        }

        public static void MultiplyFloat(Query query, string key, float multiplication, StateSource source)
        {
            query.Get<float>(key, source, out var current);
            query.Set(key, current * multiplication, source);
        }

        public static void DivideFloat(Query query, string key, float division, StateSource source)
        {
            query.Get<float>(key, source, out var current);
            query.Set(key, current / division, source);
        }

        #endregion
    }
}
