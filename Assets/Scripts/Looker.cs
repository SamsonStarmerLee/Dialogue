using Assets.Scripts.Notifications;
using Framework.Maths;
using Queries;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Looker : MonoBehaviour, IMemoryRetainer
    {
        /// <summary>
        /// How many seconds between each poll of the environment.
        /// </summary>
        [SerializeField] private float lookFrequency;

        /// <summary>
        /// This character's subtitle color when speaking.
        /// </summary>
        [SerializeField] private Color subtitleColor;

        private float timeOfLastLook;

        public Dictionary<string, object> Memory { get; } = new Dictionary<string, object>();

        private void Update()
        {
            if (Utility.Elapsed(timeOfLastLook, lookFrequency))
            {
                timeOfLastLook = Time.time;

                var ray = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out var hit))
                { 
                    var targetMemory = hit.transform.GetMemory();
                    var targetSeen = false;

                    // Memory-holding targets may store whether they have been seen previously.
                    if (targetMemory != null && targetMemory.TryGetValue("TargetSeen", out var value))
                    {
                        targetSeen = (bool)value;
                    }

                    var @event = new Dictionary<string, object>
                    {
                        { "Target", hit.transform.gameObject },
                        { "TargetName", hit.transform.name },
                        { "TargetSeen", targetSeen },
                    };

                    var character = GetCharacterState();

                    var args = new QueryArgs("SeeObject", "Player", @event, character, Memory);
                    this.PostNotification(Notify.Action<QueryArgs>(), args);
                }
            }
        }

        private Dictionary<string, object> GetCharacterState()
        {
            return new Dictionary<string, object>
            {
                { "Position", transform.position },
                { "SubtitleColor", subtitleColor },
            };
        }
    }
}

