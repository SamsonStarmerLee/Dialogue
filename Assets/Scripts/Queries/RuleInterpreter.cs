using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Queries.Subtitles;
using Criteria;
using System.Linq;
using Remember;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Assets.Scripts.Queries.Rule>>;

// TODO: Explain how to write a rule
// TODO: Explain why checked values populate memory.

// Reserved words and characters:
// Operators: '+', '-', '=', '*', '/', '>', '<', '!'
// Words: 'timestamp', 'true', 'false'
// Floating point: '.'
// Target is shorthand
// We don't allow criteria on 'Target' because that would attach memory to everything we look at

namespace Assets.Scripts.Queries
{
    sealed class RuleFixture
    {
        public string Concept;
        public string Who;

        // Criterions
        public string EventCriteria;
        public string CharacterCriteria;
        public string MemoryCriteria;
        public string WorldCriteria;

        // Rememberers
        public string RememberMemory;
        public string RememberWorld;
        public string RememberTarget;

        // Successful rule response
        public string Response;
    }

    sealed class RuleInterpreter
    {
        static readonly char[] dividers = { ',', ' ' };

        public static RuleMap Interpret()
        {
            const string ruleJson = @"
            [
                  {
                        ""Concept"": ""SeeObject"",
                        ""Who"": ""Player"",
                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
                        ""MemoryCriteria"": ""isSeenBarrels=0"",
                        ""Response"": ""Oh look! A barrel!"",
                        ""RememberMemory"": ""setSeenBarrels+1, setTimeStampBarrelComment=TimeStamp"",
                        ""RememberTarget"": ""setTargetSeen=true"",
                  },
                  {
                        ""Concept"": ""SeeObject"",
                        ""Who"": ""Player"",
                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
                        ""MemoryCriteria"": ""isSeenBarrels=1"",
                        ""Response"": ""A second barrel... how curious."",
                        ""RememberMemory"": ""setSeenBarrels+1, setTimeStampBarrelComment=TimeStamp"",
                        ""RememberTarget"": ""setTargetSeen=true"",
                  },
                  {
                        ""Concept"": ""SeeObject"",
                        ""Who"": ""Player"",
                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
                        ""MemoryCriteria"": ""isSeenBarrels=2"",
                        ""Response"": ""A <i>third</i> barrel?! Surely not!"",
                        ""RememberMemory"": ""setSeenBarrels+1, setTimeStampBarrelComment=TimeStamp"",
                        ""RememberTarget"": ""setTargetSeen=true"",
                  },
                  {
                        ""Concept"": ""SeeObject"",
                        ""Who"": ""Player"",
                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
                        ""MemoryCriteria"": ""isSeenBarrels>2"",
                        ""Response"": ""More barrels."",
                        ""RememberMemory"": ""setSeenBarrels+1, setTimeStampBarrelComment=TimeStamp"",
                        ""RememberTarget"": ""setTargetSeen=true"",
                  },
            ]";

            var fixtures = JsonConvert.DeserializeObject<RuleFixture[]>(ruleJson);
            var ruleMap = new RuleMap();
            var toOrganize = new List<List<Rule>>();

            foreach (var fixture in fixtures)
            {
                var eventCriteria     = ParseCriteriaCodes(fixture.EventCriteria, StateSource.Event);
                var characterCriteria = ParseCriteriaCodes(fixture.CharacterCriteria, StateSource.Character);
                var memoryCriteria    = ParseCriteriaCodes(fixture.MemoryCriteria, StateSource.Memory);
                var worldCriteria     = ParseCriteriaCodes(fixture.WorldCriteria, StateSource.World);
                var memoryRememberers = ParseRemembererCodes(fixture.RememberMemory, StateSource.Memory);
                var worldRememberers  = ParseRemembererCodes(fixture.RememberWorld, StateSource.World);
                var targetRememberers = ParseRemembererCodes(fixture.RememberTarget, StateSource.Target);

                var ruleKey = (fixture.Concept, fixture.Who);
                if (!ruleMap.ContainsKey(ruleKey))
                {
                    ruleMap[ruleKey] = new List<Rule>();
                    toOrganize.Add(ruleMap[ruleKey]);
                }

                var criteria = eventCriteria
                   .Concat(characterCriteria)
                   .Concat(memoryCriteria)
                   .Concat(worldCriteria)
                   .ToArray();

                var remember = memoryRememberers
                    .Concat(worldRememberers)
                    .Concat(targetRememberers)
                    .ToArray();

                ruleMap[ruleKey].Add(new GeneratedRule(
                    criteria,
                    remember,
                    fixture.Response));
            }

            // Sort each collection of rules in decending number of criteria,
            // i.e. We want to take more specific rules first.
            foreach (var l in toOrganize)
            {
                l.Sort((x, y) => y.NumCriteria.CompareTo(x.NumCriteria));
            }

            return ruleMap;
        }

        private static IEnumerable<ICriterion> ParseCriteriaCodes(string criteria, StateSource source)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                return Enumerable.Empty<ICriterion>();
            }

            return criteria
                .Split(dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretCriteriaCode(x, source));
        }

        private static IEnumerable<IRememberer> ParseRemembererCodes(string rememberers, StateSource source)
        {
            if (string.IsNullOrWhiteSpace(rememberers))
            {
                return Enumerable.Empty<IRememberer>();
            }

            return rememberers
                .Split(dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretRememberCode(x, source));
        }

        struct OperatorSplit
        {
            public string Key;
            public char Operator;
            public string Value;
        }

        private static bool SplitOnOperator(string raw, out OperatorSplit split, params char[] operators)
        {
            var index = raw.IndexOfAny(operators);

            if (index == -1)
            {
                split = default;
                return false;
            }

            var key = raw.Substring(0, index);
            var op = raw[index];
            var value = raw.Substring(index + 1);

            split = new OperatorSplit
            {
                Key = key,
                Operator = op,
                Value = value
            };

            return true;
        }

        #region Is Operators

        private static ICriterion InterpretCriteriaCode(string code, StateSource source)
        {
            if (code.StartsWith("is"))
            {
                var subCode = code.Remove(0, 2);

                if (SplitOnOperator(subCode, out var split, '=', '>', '<', '!'))
                {
                    var key = split.Key;
                    var value = split.Value;
                    var op = split.Operator;

                    // Check for bool
                    // NOTE: Because of this, 'true' and 'false' are reserved strings.
                    if (op == '=' && value == "true" || value == "false")
                    {
                        return IsBool(split, source);
                    }

                    // Check for a number 
                    // Any value containing a decimal is a float check.
                    else if (value.Contains('.'))
                    {
                         return IsFloat(split, source);
                    }

                    // A value containing all digits is an int check.
                    else if (value.All(char.IsDigit))
                    {
                        return IsInt(split, source);
                    }

                    // String comparison.
                    else if (op == '=')
                    {
                        return new IsEqual<string>(split.Key, split.Value, source);
                    }
                }
            }
            else
            {
                // Custom operation.
                return CustomCriteria(code);
            }

            // Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }

        private static ICriterion IsInt(OperatorSplit split, StateSource source)
        {
            var i = int.Parse(split.Value);

            switch (split.Operator)
            {
                case '=':
                    return new IsEqual<int>(split.Key, i, source);
                case '>':
                    return new IsGreater<int>(split.Key, i, source);
                case '<':
                    return new IsLess<int>(split.Key, i, source);
                case '!':
                    return new IsNotEqual<int>(split.Key, i, source);
                default:
                    Debug.LogError($"Couldn't interpret criteria as int operation: {split.Key}, {split.Operator}, {split.Value}.");
                    return null;
            }
        }

        private static ICriterion IsFloat(OperatorSplit split, StateSource source)
        {
            var f = float.Parse(split.Value);

            switch (split.Operator)
            {
                case '=':
                    return new IsEqual<float>(split.Key, f, source);
                case '>':
                    return new IsGreater<float>(split.Key, f, source);
                case '<':
                    return new IsLess<float>(split.Key, f, source);
                case '!':
                    return new IsNotEqual<float>(split.Key, f, source);
                default:
                    Debug.LogError($"Couldn't interpret criteria as float operation: {split.Key}, {split.Operator}, {split.Value}.");
                    return null;
            }
        }

        private static ICriterion IsBool(OperatorSplit split, StateSource source)
        {
            var b = bool.Parse(split.Value);
            return new IsEqual<bool>(split.Key, b, source);
        }

        private static ICriterion CustomCriteria(string code)
        {
            var type = Type.GetType($"Criteria.{code}");

            if (type == null)
            {
                Debug.LogError($"Failed to find criteria class for: {code}.");
                return null;
            }

            return (ICriterion)Activator.CreateInstance(type);
        }

        #endregion

        #region Set Operators

        private static IRememberer InterpretRememberCode(string code, StateSource source)
        {
            if (code.StartsWith("set"))
            {
                var subCode = code.Remove(0, 3);

                if (SplitOnOperator(subCode, out var split, '=', '-', '+', '*', '/'))
                {
                    var key = split.Key;
                    var value = split.Value;
                    var op = split.Operator;

                    /// Bool assignment.
                    /// NOTE: Because of this, 'true' and 'false' are reserved strings.
                    if (op == '=' && value == "true" || value == "false")
                    {
                        return SetBool(split, source);
                    }

                    /// Check for a number 
                    /// Any value containing a decimal is a float manipulation.
                    else if (value.Contains('.'))
                    {
                        return SetFloat(split, source);
                    }

                    /// A value containing all digits is an int manipulation.
                    else if (value.All(char.IsDigit))
                    {
                        return SetInt(split, source);
                    }

                    /// String assignment.
                    else if (op == '=')
                    {
                        return new Set(split.Key, split.Value, source);
                    }
                }
            }
            else
            {
                /// Custom operation.
                return CustomRemember(code);
            }

            /// Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }

        private static IRememberer SetInt(OperatorSplit split, StateSource source)
        {
            var i = int.Parse(split.Value);

            switch (split.Operator)
            {
                case '=':
                    return new Set(split.Key, i, source);
                case '-':
                    return new SubtractInt(split.Key, i, source);
                case '+':
                    return new AddInt(split.Key, i, source);
                case '*':
                    Debug.LogError($"Integer multiplication not supported: {split}. Add a decimal.");
                    return null;
                case '/':
                    Debug.LogError($"Integer division not supported: {split}. Add a decimal.");
                    return null;
                default:
                    Debug.LogError($"Couldn't interpret criteria as int operation: {split}.");
                    return null;
            }
        }

        private static IRememberer SetFloat(OperatorSplit split, StateSource source)
        {
            float f;

            if (split.Value.ToLowerInvariant() == "timestamp")
            {
                f = Time.time;
            }
            else
            {
                f = float.Parse(split.Value);
            }

            switch (split.Operator)
            {
                case '=':
                    return new Set(split.Key, f, source);
                case '-':
                    return new SubtractFloat(split.Key, f, source);
                case '+':
                    return new AddFloat(split.Key, f, source);
                case '*':
                    return new MultiplyFloat(split.Key, f, source);
                case '/':
                    return new DivideFloat(split.Key, f, source);
                default:
                    Debug.LogError($"Couldn't interpret criteria as float operation: {split}.");
                    return null;
            }
        }

        private static IRememberer SetBool(OperatorSplit split, StateSource source)
        {
            var b = bool.Parse(split.Value);
            return new Set(split.Key, b, source);
        }

        private static IRememberer CustomRemember(string code)
        {
            var type = Type.GetType($"Remember.{code}");

            if (type == null)
            {
                Debug.LogError($"Failed to find memory class for: {code}.");
                return null;
            }

            return (IRememberer)Activator.CreateInstance(type);
        }

        #endregion
    }

    sealed class GeneratedRule : Rule
    {
        private readonly ICriterion[] criteria;
        private readonly IRememberer[] rememberers;
        private readonly string response;

        public GeneratedRule(
            ICriterion[] criteria, 
            IRememberer[] rememberers, 
            string response)
        {
            this.criteria = criteria;
            this.rememberers = rememberers;
            this.response = response;
        }

        public override ICriterion[] Criteria => criteria;

        public override IRememberer[] Rememberers => rememberers;

        public override void Response(Query query)
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






//const string ruleJson = @"
//            [
//                  {
//                        ""Concept"": ""SeeObject"",
//                        ""Who"": ""Player"",
//                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
//                        ""CharacterCriteria"": """",
//                        ""MemoryCriteria"": ""isSeenBarrels=0"",
//                        ""WorldCriteria"": """",
//                        ""Response"": ""Oh look! A barrel!"",
//                        ""RememberMemory"": ""RememberBarrel"",
//                        ""RememberWorld"": """"
//                  },
//                  {
//                        ""Concept"": ""SeeObject"",
//                        ""Who"": ""Player"",
//                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
//                        ""CharacterCriteria"": """",
//                        ""MemoryCriteria"": ""isSeenBarrels=1"",
//                        ""WorldCriteria"": """",
//                        ""Response"": ""A second barrel... how curious."",
//                        ""RememberMemory"": ""RememberBarrel"",
//                        ""RememberWorld"": """"
//                  },
//                  {
//                        ""Concept"": ""SeeObject"",
//                        ""Who"": ""Player"",
//                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
//                        ""CharacterCriteria"": """",
//                        ""MemoryCriteria"": ""isSeenBarrels=2"",
//                        ""WorldCriteria"": """",
//                        ""Response"": ""A <i>third</i> barrel?! Surely not!"",
//                        ""RememberMemory"": ""RememberBarrel"",
//                        ""RememberWorld"": """"
//                  },
//                  {
//                        ""Concept"": ""SeeObject"",
//                        ""Who"": ""Player"",
//                        ""EventCriteria"": ""isTargetName=Barrel, TargetNotSeen"",
//                        ""CharacterCriteria"": """",
//                        ""MemoryCriteria"": ""isSeenBarrels>2"",
//                        ""WorldCriteria"": """",
//                        ""Response"": ""More barrels."",
//                        ""RememberMemory"": ""RememberBarrel"",
//                        ""RememberWorld"": """"
//                  },
//            ]";