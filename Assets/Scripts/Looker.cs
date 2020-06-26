using Assets.Scripts.Queries;
using Dialogue;
using System.Collections.Generic;
using UnityEngine;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Assets.Scripts.Queries.Rule>>;

namespace Assets.Scripts
{
    public class Looker : MonoBehaviour, IMemoryRetainer
    {
        [SerializeField] private Color subtitleColor;

        private RuleMap rules;
        private float timeOfLastInspection;

        public Dictionary<string, object> Memory { get; } = new Dictionary<string, object>();

        private void Start()
        {
            rules = RuleInterpreter.Interpret();
        }

        private void Update()
        {
            timeOfLastInspection += Time.deltaTime;

            // Poll view for things to comment on.
            if (timeOfLastInspection >= 1f)
            {
                timeOfLastInspection = 0f;

                var ray = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out var hit))
                {
                    var targetMemory = hit.transform.GetMemory();
                    var targetSeen = false;

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

                    var character = GenerateCharacterState();

                    QueryManager
                        .Instance
                        .Announce("SeeObject", "Player", @event, character, Memory, rules);
                }
            }
        }

        private Dictionary<string, object> GenerateCharacterState()
        {
            return new Dictionary<string, object>
            {
                { "Position", transform.position },
                { "SubtitleColor", subtitleColor },
            };
        }
    }
}

