using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Queries.Subtitles;
using Criteria;
using System.Linq;
using Remember;
using Framework.Maths;
using Assets.Scripts.Notifications;

namespace Assets.Scripts.Queries
{
    sealed class Rule
    {
        private readonly string response;

        public Rule(
            int id,
            IEnumerable<ICriterion> criteria, 
            IEnumerable<Rememberer> rememberers, 
            string response,
            float? cooldown)
        {
            Id = id;
            Criteria = criteria.ToList();
            Rememberers = rememberers.ToList();
            this.response = response;
            Cooldown = cooldown ?? -1;
        }

        /// <summary>
        /// Unique Id to this rule.
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// The minimum required gap before this rule can re-fire.
        /// </summary>
        public float Cooldown { get; }

        public IReadOnlyList<ICriterion> Criteria { get; }

        public IReadOnlyList<Rememberer> Rememberers { get; }

        public int NumCriteria => Criteria.Count;

        /// <summary>
        /// Whether or not this rule can only fire once.
        /// </summary>
        public bool OneShot => Cooldown == -1;

        /// <summary>
        /// Has this rule triggered once before?
        /// </summary>
        public bool Triggered { get; private set; }

        /// <summary>
        /// If this rule has triggered, the timespan on when it did is recorded here.
        /// </summary>
        public float TriggerTimeStamp { get; private set; }

        /// <summary>
        /// Evaluate criteria and return true if all pass.
        /// </summary>
        public bool Evaluate(Query query)
        {
            foreach (var c in Criteria)
            {
                if (!c.Eval(query))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Any state to store/manipulate as a result of this rule's successful passing.
        /// </summary>
        public void Remember(Query query)
        {
            foreach (var rememberer in Rememberers)
            {
                rememberer.Invoke(query);
            }

            Triggered = true;
            TriggerTimeStamp = Time.time;
        }

        /// <summary>
        /// Actions to take in response to stimulus.
        /// </summary>
        public void Response(Query query)
        {
            query.Get<Color>("SubtitleColor", StateSource.Character, out var color);
            var args = new SubtitleArgs(query.Who, response, color);
            this.PostNotification(Notify.Action<SubtitleArgs>(), args);
        }
    }
}