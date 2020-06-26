using Handler = System.Action<object, object>;

namespace Notifications
{
    //private void Awake()
    //{
    //    this.AddObserver(OnDialogueEvent, "OnDialogueEvent");
    //}

    //private void OnDestroy()
    //{
    //    this.RemoveObserver(OnDialogueEvent, "OnDialogueEvent");
    //}

    //private void OnDialogueEvent(object sender, object args)
    //{
    //    // Do something
    //}

    public static class NotificationExtensions
    {
        public static void PostNotification(this object @this, string notificationName)
        {
            NotificationManager.Instance.PostNotification(notificationName, @this);
        }

        public static void PostNotification(this object @this, string notificationName, object e)
        {
            NotificationManager.Instance.PostNotification(notificationName, @this, e);
        }

        public static void AddObserver(this object @this, Handler handler, string notificationName)
        {
            NotificationManager.Instance.AddObserver(handler, notificationName);
        }

        public static void AddObserver(this object @this, Handler handler, string notificationName, object sender)
        {
            NotificationManager.Instance.AddObserver(handler, notificationName, sender);
        }

        public static void RemoveObserver(this object @this, Handler handler, string notificationName)
        {
            NotificationManager.Instance.RemoveObserver(handler, notificationName);
        }

        public static void RemoveObserver(this object @this, Handler handler, string notificationName, object sender)
        {
            NotificationManager.Instance.RemoveObserver(handler, notificationName, sender);
        }
    }
}
