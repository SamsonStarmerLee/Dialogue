using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Queries.Subtitles;
using Criteria;
using System.Linq;
using Remember;
using Framework.Maths;

namespace Assets.Scripts.Queries
{
    sealed class Rule
    {
        private readonly string response;

        public Rule(
            int id,
            IEnumerable<ICriterion> criteria, 
            IEnumerable<IRememberer> rememberers, 
            string response,
            float? cooldown)
        {
            Id = id;
            Criteria = criteria.ToList();
            Rememberers = rememberers.ToList();
            this.response = response;
            Cooldown = cooldown ?? -1;
        }

        public int Id { get; }
        
        public float Cooldown { get; }

        public IReadOnlyList<ICriterion> Criteria { get; }

        public IReadOnlyList<IRememberer> Rememberers { get; }

        public int NumCriteria => Criteria.Count;

        public bool OneShot => Cooldown == -1;

        public bool Triggered { get; private set; }

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
                rememberer.Apply(query);
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

            var subtitle = new SubtitleRequest()
            {
                Speaker = query.Who,
                Text = response,
                Color = color
            };

            // TEMP
            var subtitleManager = GameObject.FindObjectOfType<SubtitleManager>();
            subtitleManager.DisplaySubtitle(subtitle);
        }
    }
}