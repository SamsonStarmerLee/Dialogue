using System;
using System.Collections.Generic;
using UnityEngine;
using Criteria;
using System.Linq;
using Remember;
using System.IO;
using CsvHelper;
using System.Globalization;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Assets.Scripts.Queries.Rule>>;

// TODO: Explain why checked values populate memory.

// Reserved words and characters:

// Criteria Values
// 'true', 'false' refer to the boolean states.
// Any value containing a '.' indicates a float

// Remember Values
// Events like SeeObject include a 'Target', the memory of this target object is accessed with this same code.
// We don't allow criteria on 'Target' because that would attach memory to everything we look at.

// Either
// All the used operators: '+', '-', '=', '*', '/', '>', '<', '!'
// 'Rule' populates with the Id of the current rule.
// 'Timestamp' populates the current time.

// Character-sourced key 'RecentRules' reserved for an ordered list of successful rule ids.

// TODO: Right now, custom criteria/remememberers are found using reflection, making them slow to do many times. 
// Perhaps replace with a dictionary or something.

namespace Assets.Scripts.Queries
{
    sealed class RuleInterpreter
    {
        static readonly string[] Dividers = { Environment.NewLine, "\r\n", "\n" };

        sealed class RuleFixture
        {
            public int Id { get; set; }

            public string Concept { get; set; }
            
            public string Who { get; set; }

            public string Response { get; set; }
            
            public string Criteria { get; set; }

            public string Remember { get; set; }

            public float? Cooldown { get; set; }
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
                    var id = fixture.Id;
                    var criteria = ParseCriteriaCodes(id, fixture.Criteria);
                    var remember = ParseRemembererCodes(id, fixture.Remember);

                    var ruleKey = (fixture.Concept, fixture.Who);
                    if (!ruleMap.ContainsKey(ruleKey))
                    {
                        ruleMap[ruleKey] = new List<Rule>();
                        toOrganize.Add(ruleMap[ruleKey]);
                    }

                    ruleMap[ruleKey].Add(new Rule(
                        id, 
                        criteria, 
                        remember, 
                        fixture.Response,
                        fixture.Cooldown));
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

        private static IEnumerable<ICriterion> ParseCriteriaCodes(int id, string criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                return Enumerable.Empty<ICriterion>();
            }

            return criteria
                .Split(Dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretCriteriaCode(id, x));
        }

        private static IEnumerable<Rememberer> ParseRemembererCodes(int id, string rememberers)
        {
            if (string.IsNullOrWhiteSpace(rememberers))
            {
                return Enumerable.Empty<Rememberer>();
            }

            return rememberers
                .Split(Dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretRememberCode(id, x));
        }

        struct RuleSplit
        {
            public StateSource Source;
            public string Key;
            public char Operator;
            public string Value;
        }

        static readonly Dictionary<char, StateSource> SourceCodes = new Dictionary<char, StateSource>()
        {
            { 'e', StateSource.Event },
            { 'c', StateSource.Character },
            { 'm', StateSource.Memory },
            { 'w', StateSource.World },
            { 't', StateSource.Target },
            { '@', StateSource.Custom },
        };

        private static bool SplitOnOperator(int id, string raw, out RuleSplit split, params char[] operators)
        {
            var index = raw.IndexOfAny(operators);
            if (index == -1)
            {
                split = default;
                return false;
            }

            var source = SourceCodes[raw[0]];
            var key = raw.Substring(1, index - 1);
            var op = raw[index];
            var value = raw.Substring(index + 1);

            // Handle reserved words/characters
            {
                if (value.ToLowerInvariant() == "timestamp")
                {
                    value = Time.time.ToString();
                } 

                if (value.ToLowerInvariant() == "rule")
                {
                    value = id.ToString();
                }
            }

            split = new RuleSplit
            {
                Source = source,
                Key = key,
                Operator = op,
                Value = value,
            };

            return true;
        }

        #region Is Operators

        private static ICriterion InterpretCriteriaCode(int id, string code)
        {
            if (SplitOnOperator(id, code, out var split, '=', '>', '<', '!'))
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

        private static Rememberer InterpretRememberCode(int id, string code)
        {
            if (SplitOnOperator(id, code, out var split, '=', '-', '+', '*', '/'))
            {
                if (split.Source == StateSource.Custom)
                {
                    return CustomRemember(code);
                }

                var source = split.Source;
                var key = split.Key;
                var value = split.Value;
                var op = split.Operator;

                // Bool assignment.
                // NOTE: Because of this, 'true' and 'false' are reserved strings.
                if (op == '=' && value == "true" || value == "false")
                {
                    return SetBool(split, source);
                }

                // Check for a number 
                // Any value containing a decimal is a float manipulation.
                else if (value.Contains('.'))
                {
                    return SetFloat(split, source);
                }

                // A value containing all digits is an int manipulation.
                else if (value.All(char.IsDigit))
                {
                    return SetInt(split, source);
                }

                // String assignment.
                else if (op == '=')
                {
                    return (query) => 
                        Rememberers.Set(query, split.Key, split.Value, source);
                }
            }

            // Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }

        private static Rememberer SetInt(RuleSplit split, StateSource source)
        {
            var @int = int.Parse(split.Value);

            switch (split.Operator)
            {
                case '=':
                    return (query) => Rememberers.Set(query, split.Key, @int, source);
                case '-':
                    return (query) => Rememberers.SubtractInt(query, split.Key, @int, source);
                case '+':
                    return (query) => Rememberers.AddInt(query, split.Key, @int, source);
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

        private static Rememberer SetFloat(RuleSplit split, StateSource source)
        {
            var @float = float.Parse(split.Value);

            switch (split.Operator)
            {
                case '=':
                    return (query) => Rememberers.Set(query, split.Key, @float, source);
                case '-':
                    return (query) => Rememberers.SubtractFloat(query, split.Key, @float, source);
                case '+':
                    return (query) => Rememberers.AddFloat(query, split.Key, @float, source);
                case '*':
                    return (query) => Rememberers.MultiplyFloat(query, split.Key, @float, source);
                case '/':
                    return (query) => Rememberers.DivideFloat(query, split.Key, @float, source);
                default:
                    Debug.LogError($"Couldn't interpret criteria as float operation: {split}.");
                    return null;
            }
        }

        private static Rememberer SetBool(RuleSplit split, StateSource source)
        {
            var @bool = bool.Parse(split.Value);
            return (query) => Rememberers.Set(query, split.Key, @bool, source);
        }

        private static Rememberer CustomRemember(string code)
        {
            // TODO
            return null;
        }

        #endregion
    }
}