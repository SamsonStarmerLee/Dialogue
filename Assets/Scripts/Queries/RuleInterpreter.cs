using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Queries.Subtitles;
using Criteria;
using System.Linq;
using Remember;
using System.IO;
using CsvHelper;
using System.Globalization;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Assets.Scripts.Queries.Rule>>;

// TODO: Explain how to write a rule
// TODO: Explain why checked values populate memory.

// Reserved words and characters:
// Operators: '+', '-', '=', '*', '/', '>', '<', '!'
// Words: 'timestamp', 'true', 'false'
// Floating point: '.'
// Target is shorthand
// We don't allow criteria on 'Target' because that would attach memory to everything we look at

// TODO: Right now, custom criteria/remememberers are found using reflection, making them slow to do many times. 
// Perhaps replace with a dictionary or something.

namespace Assets.Scripts.Queries
{
    sealed class RuleInterpreter
    {
        static readonly string[] dividers = { Environment.NewLine, "\r\n", "\n" };

        sealed class RuleFixture
        {
            public string Concept { get; set; }
            
            public string Who { get; set; }

            public string Response { get; set; }
            
            public string Criteria { get; set; }

            public string Remember { get; set; }
        }

        public static RuleMap Interpret()
        {
            var ruleMap = new RuleMap();
            var toOrganize = new List<List<Rule>>();

            using (var reader = new StreamReader(Application.dataPath + "/Resources/Dialogue/csvtest.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var fixtures = csv.GetRecords<RuleFixture>();

                foreach (var fixture in fixtures)
                {
                    var criteria = ParseCriteriaCodes(fixture.Criteria);
                    var remember = ParseRemembererCodes(fixture.Remember);

                    var ruleKey = (fixture.Concept, fixture.Who);
                    if (!ruleMap.ContainsKey(ruleKey))
                    {
                        ruleMap[ruleKey] = new List<Rule>();
                        toOrganize.Add(ruleMap[ruleKey]);
                    }

                    ruleMap[ruleKey].Add(new GeneratedRule(
                        criteria.ToArray(),
                        remember.ToArray(),
                        fixture.Response));
                }
            }

            // Sort each collection of rules in decending number of criteria,
            // i.e. We want to take more specific rules first.
            foreach (var l in toOrganize)
            {
                l.Sort((x, y) => y.NumCriteria.CompareTo(x.NumCriteria));
            }

            return ruleMap;
        }

        private static IEnumerable<ICriterion> ParseCriteriaCodes(string criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                return Enumerable.Empty<ICriterion>();
            }

            return criteria
                .Split(dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretCriteriaCode(x));
        }

        private static IEnumerable<IRememberer> ParseRemembererCodes(string rememberers)
        {
            if (string.IsNullOrWhiteSpace(rememberers))
            {
                return Enumerable.Empty<IRememberer>();
            }

            return rememberers
                .Split(dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretRememberCode(x));
        }

        struct RuleSplit
        {
            public StateSource Source;
            public string Key;
            public char Operator;
            public string Value;
        }

        private static bool SplitOnOperator(string raw, out RuleSplit split, params char[] operators)
        {
            var index = raw.IndexOfAny(operators);
            if (index == -1)
            {
                split = default;
                return false;
            }

            var sourceCodes = new Dictionary<char, StateSource>() 
            {
                { 'e', StateSource.Event },
                { 'c', StateSource.Character },
                { 'm', StateSource.Memory },
                { 'w', StateSource.World },
                { 't', StateSource.Target },
                { '*', StateSource.Custom },
            };

            split = new RuleSplit
            {
                Source = sourceCodes[raw[0]],
                Key = raw.Substring(1, index - 1),
                Operator = raw[index],
                Value = raw.Substring(index + 1)
            };

            return true;
        }

        #region Is Operators

        private static ICriterion InterpretCriteriaCode(string code)
        {
            if (SplitOnOperator(code, out var split, '=', '>', '<', '!'))
            {
                if (split.Source == StateSource.Custom)
                {
                    return CustomCriteria(code);
                }

                var source = split.Source;
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

            // Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }

        private static ICriterion IsInt(RuleSplit split, StateSource source)
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

        private static ICriterion IsFloat(RuleSplit split, StateSource source)
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

        private static ICriterion IsBool(RuleSplit split, StateSource source)
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

        private static IRememberer InterpretRememberCode(string code)
        {
            if (SplitOnOperator(code, out var split, '=', '-', '+', '*', '/'))
            {
                if (split.Source == StateSource.Custom)
                {
                    return CustomRemember(code);
                }

                var source = split.Source;
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

            /// Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }

        private static IRememberer SetInt(RuleSplit split, StateSource source)
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

        private static IRememberer SetFloat(RuleSplit split, StateSource source)
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

        private static IRememberer SetBool(RuleSplit split, StateSource source)
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