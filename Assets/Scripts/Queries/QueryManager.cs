using Assets.Scripts.Queries;
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
        /// Generates and sends a query to a specific character.
        /// </summary>
        // TODO: Get rules from character or something rather than pass in.
        public void Announce(
            string concept,
            string who,
            Dictionary<string, object> @event,
            Dictionary<string, object> character,
            Dictionary<string, object> memory, 
            RuleMap rules)
        {
            var world = GenerateWorldMemory();
            var query = new Query(concept, who, @event, character, memory, world);

            HandleQuery(query, rules);
        }

        private void HandleQuery(Query query, RuleMap rulesMap)
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

                else if (rule.Evaluate(query))
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
            }
        }

        // TODO: Have actual world memory come from somewhere.
        private Dictionary<string, object> GenerateWorldMemory()
        {
            return new Dictionary<string, object>()
            {
                { "Time", Time.deltaTime }
            };
        }
    }
}
