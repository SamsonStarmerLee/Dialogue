using Assets.Scripts.Notifications;
using Assets.Scripts.Queries;
using Framework.Maths;
using System;
using System.Collections.Generic;
using UnityEngine;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Assets.Scripts.Queries.Rule>>;

namespace Dialogue
{
    public sealed class QueryArgs
    {
        public string Concept { get; }
        public string Who { get; }
        public Dictionary<string, object> Event { get; }
        public Dictionary<string, object> Character { get; }
        public Dictionary<string, object> Memory { get; }

        public QueryArgs(string concept, 
            string who, 
            Dictionary<string, object> @event, 
            Dictionary<string, object> character, 
            Dictionary<string, object> memory)
        {
            Concept = concept;
            Who = who;
            Event = @event;
            Character = character;
            Memory = memory;
        }
    }

    public sealed class QueryManager : MonoBehaviour
    {
        private RuleMap rules;
        private List<Rule> oneshots;
        private Dictionary<string, object> worldMemory;

        private void Awake()
        {
            rules = RuleInterpreter.Interpret();
            oneshots = new List<Rule>();

            worldMemory = new Dictionary<string, object>()
            {
                { "Time", Time.deltaTime }
            };
        }

        private void OnEnable()
        {
            this.AddObserver(OnQueryEvent, Notify.Action<QueryArgs>());
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnQueryEvent, Notify.Action<QueryArgs>());
        }

        private void OnQueryEvent(object sender, object args)
        {
            UpdateWorldMemory();
            var query = new Query(args as QueryArgs, worldMemory);
            HandleQuery(query, rules, oneshots);
        }

        private void UpdateWorldMemory()
        {
            worldMemory["Time"] = Time.deltaTime;
        }

        private void HandleQuery(
            Query query, 
            RuleMap rulesMap,
            List<Rule> oneshots)
        {
            if (!rulesMap.ContainsKey((query.Concept, query.Who)))
            {
                return;
            }

            var rules = rulesMap[(query.Concept, query.Who)];
            var passes = new List<Rule>();
            var numCriteria = 0;

            foreach (var rule in rules)
            {
                if (rule.NumCriteria < numCriteria)
                {
                    // Take only rules with the greatest
                    // number of criteria.
                    break;
                }

                if (rule.Triggered && !Utility.Elapsed(rule.TriggerTimeStamp, rule.Cooldown))
                {
                    // Skip rules on cooldown.
                    continue;
                }

                if (rule.Evaluate(query))
                {
                    passes.Add(rule);
                    numCriteria = rule.NumCriteria;
                }
            }

            if (passes.Count > 0)
            {
                // Select the rule to execute randomly.
                var index = UnityEngine.Random.Range(0, passes.Count);
                var rule = passes[index];

                rule.Response(query);
                rule.Remember(query);

                if (rule.OneShot)
                {
                    // Remove oneshot rules from the rulemap.
                    rules.Remove(rule);
                    oneshots.Add(rule);
                }
            }
        }
    }
}
