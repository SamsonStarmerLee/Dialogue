using Assets.Scripts.Queries;
using Framework.Maths;
using System.Collections.Generic;
using UnityEngine;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Assets.Scripts.Queries.Rule>>;

namespace Dialogue
{
    class QueryManager
    {
        public static QueryManager Instance { get; } = new QueryManager();

        // TODO (send query to many/specific actors).
        // Valve defined speak targets as:
        // - specific actors
        // - any actor (other actors)
        // - all actors (including self)

        /// <summary>
        /// Searches for a rule matching the given scenario, executing its response.
        /// </summary>
        public void Announce(
            string concept,
            string who,
            Dictionary<string, object> @event,
            Dictionary<string, object> character,
            Dictionary<string, object> memory, 
            RuleMap rules,
            List<Rule> oneshots)
        {
            var world = GetWorldMemory();
            var query = new Query(concept, who, @event, character, memory, world);

            HandleQuery(query, rules, oneshots);
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
                var index = Random.Range(0, passes.Count);
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

        // TODO: Have actual world memory come from somewhere.
        private Dictionary<string, object> GetWorldMemory()
        {
            return new Dictionary<string, object>()
            {
                { "Time", Time.deltaTime }
            };
        }
    }
}
