
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Notifications
{
    /// <summary>
    /// This delegate is similar to an EventHandler:
    ///     The first parameter is the sender,
    ///     The second parameter is the arguments / info to pass.
    /// </summary>
    using Handler = Action<object, object>;

    /// <summary>
    /// The SenderTable maps from an object (sender of a notification),
    /// to a list of Handler methods
    ///     * Note - when no sender is specified for the SenderTable,
    ///     the NotificationCenter itself is used at the sender key.
    /// </summary>
    using SenderTable = Dictionary<object, List<Action<object, object>>>;

    public class NotificationManager
    {
        #region Singleton Pattern

        public readonly static NotificationManager Instance = new NotificationManager();
        private NotificationManager() { }

        #endregion

        private Dictionary<string, SenderTable> table = new Dictionary<string, SenderTable>();
        private HashSet<List<Handler>> invoking = new HashSet<List<Handler>>();

        public void AddObserver(Handler handler, string notificationName)
        {
            AddObserver(handler, notificationName, null);
        }

        public void AddObserver(Handler handler, string notificationName, object sender)
        {
            if (handler == null)
            {
                Debug.LogError($"Can't add a null event handler for notification, {notificationName}");
                return;
            }

            if (string.IsNullOrEmpty(notificationName))
            {
                Debug.LogError($"Can't observe an unnamed notification.");
                return;
            }

            // Create a sub table for this key if one doesn't already exist.
            if (!table.ContainsKey(notificationName))
            {
                table.Add(notificationName, new SenderTable());
            }

            // If the sender value is null, use the NotificationManager itself.
            var key = (sender != null) ? sender : this;
            var senders = table[notificationName];

            if (!senders.ContainsKey(key))
            {
                senders.Add(key, new List<Handler>());
            }

            var handlers = senders[key];

            // Add the handler to the subTable (if not already handling).
            if (!handlers.Contains(handler))
            {
                // If the subTable is currently invoking, add
                // the new Handler to a second list and assign it.
                if (invoking.Contains(handlers))
                {
                    senders[key] = handlers = new List<Handler>(handlers);
                }

                handlers.Add(handler);
            }
        }

        public void RemoveObserver(Handler handler, string notificationName)
        {
            RemoveObserver(handler, notificationName, null);
        }

        public void RemoveObserver(Handler handler, string notificationName, object sender)
        {
            if (handler == null)
            {
                Debug.LogError($"Can't remove a null event handler for notification,{notificationName}.");
                return;
            }

            if (string.IsNullOrEmpty(notificationName))
            {
                Debug.LogError("A notification name is required to stop observation");
                return;
            }

            // No need to take action if we don't monitor this notification.
            if (!table.ContainsKey(notificationName))
            {
                return;
            }

            var senders = table[notificationName];
            var key = (sender != null) ? sender : this;

            if (!senders.ContainsKey(key))
            {
                return;
            }

            var handlers = senders[key];
            var index = handlers.IndexOf(handler);

            // If the subTable contains the given handler, remove it.
            if (index != -1)
            {
                if (invoking.Contains(handlers))
                {
                    // Leave the invoking list in place
                    // and remove from a newly assigned list.
                    senders[key] = handlers = new List<Handler>(handlers);
                    handlers.RemoveAt(index);
                }
            }
        }

        public void PostNotification(string notificationName)
        {
            PostNotification(notificationName, null);
        }

        public void PostNotification(string notificationName, object sender)
        {
            PostNotification(notificationName, sender, null);
        }

        public void PostNotification(string notificationName, object sender, object e)
        {
            if (string.IsNullOrEmpty(notificationName))
            {
                Debug.LogError("A notification name is required.");
                return;
            }

            // No need to take action if this notification isn't being observed.
            if (!table.ContainsKey(notificationName))
            {
                return;
            }

            // Post to subscribers who specified a sender to observe.
            var subTable = table[notificationName];
            
            if (sender != null && subTable.ContainsKey(sender))
            {
                var handlers = subTable[sender];
                invoking.Add(handlers);

                for (var i = 0; i < handlers.Count; ++i)
                {
                    handlers[i](sender, e);
                }

                invoking.Remove(handlers);
            }
        }

        //public void Clean()
        //{
        //    var notKeys = new string[table.Keys.Count];
        //    table.Keys.CopyTo(notKeys, 0);

        //    for (var i = notKeys.Length - 1; i >= 0; --i)
        //    {
        //        var notificationName = notKeys[i];
        //        var senderTable = table[notificationName];

        //        var senKeys = new object[senderTable.Keys.Count];
        //        senderTable.Keys.CopyTo(senKeys, 0);

        //        for (var j = senKeys.Length - 1; j >= 0; --j)
        //        {
        //            var sender = senKeys[j];
        //            var handlers = senderTable[sender];

        //            if (handlers.Count == 0)
        //            {
        //                senderTable.Remove(sender);
        //            }
        //        }

        //        if (senderTable.Count == 0)
        //        {
        //            table.Remove(notificationName);
        //        }
        //    }
        //}
    }
}
