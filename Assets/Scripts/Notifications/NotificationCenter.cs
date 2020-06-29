using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Notifications
{
    public delegate void Handler(object sender, object args);

    public class NotificationCenter
    {
        public static NotificationCenter Instance { get; } = new NotificationCenter();

        private readonly Dictionary<string, List<Handler>> table =
            new Dictionary<string, List<Handler>>();

        public void AddObserver(Handler handler, string notificationName)
        {
            if (string.IsNullOrWhiteSpace(notificationName))
            {
                Debug.LogError("Null or empty notificationName.");
                return;
            }

            if (handler == null)
            {
                Debug.LogError("Null handler.");
                return;
            }

            if (!table.ContainsKey(notificationName))
            {
                // New observable requested. Populate the table.
                table.Add(notificationName, new List<Handler>());
            }

            var subtable = table[notificationName];
            if (!subtable.Contains(handler))
            {
                subtable.Add(handler);
            }
        }

        public void RemoveObserver(Handler handler, string notificationName)
        {
            if (string.IsNullOrWhiteSpace(notificationName))
            {
                Debug.LogError("Null or empty notificationName.");
                return;
            }

            if (handler == null)
            {
                Debug.LogError("Null handler.");
                return;
            }

            if (!table.ContainsKey(notificationName))
            {
                // Notification not registered.
                return;
            }

            var subtable = table[notificationName];
            if (subtable.Contains(handler))
            {
                subtable.Remove(handler);
            }
        }

        public void Post(object sender, object e, string notificationName)
        {
            if (string.IsNullOrWhiteSpace(notificationName))
            {
                Debug.LogError("Null or empty notificationName.");
                return;
            }

            if (!table.ContainsKey(notificationName))
            {
                // No Listeners
                return;
            }

            var subtable = table[notificationName];

            // Loop in reverse in case new handlers are added mid-invokation.
            for (var i = subtable.Count - 1; i >= 0; i--)
            {
                subtable[i].Invoke(sender, e);
            }
        }

        // TODO: Clean out all unobserved notification types.
        //public void Clean()
        //{
        //}
    }
}
