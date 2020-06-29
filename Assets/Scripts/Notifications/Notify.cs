namespace Assets.Scripts.Notifications
{
    public static class Notify
    {
        public static string Action<T>() => Action(typeof(T));

        public static string Action(System.Type type) => $"{type.Name}.Notification";
    }
}
