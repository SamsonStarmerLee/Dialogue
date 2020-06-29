namespace Assets.Scripts.Notifications
{
    public static class NotificationExtensions
    {
        public static void AddObserver(this object _, Handler handler, string notificationName)
        {
            NotificationCenter.Instance.AddObserver(handler, notificationName);
        }

        public static void RemoveObserver(this object _, Handler handler, string notificationName)
        {
            NotificationCenter.Instance.RemoveObserver(handler, notificationName);
        }

        public static void PostNotification(this object @this, string notificationName)
        {
            NotificationCenter.Instance.Post(@this, null, notificationName);
        }

        public static void PostNotification(this object @this, string notificationName, object e)
        {
            NotificationCenter.Instance.Post(@this, e, notificationName);
        }
    }
}
