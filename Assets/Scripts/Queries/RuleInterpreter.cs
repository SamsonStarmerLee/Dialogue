using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using CsvHelper;
using System.Globalization;

using RuleMap = System.Collections.Generic.Dictionary<(string concept, string who), System.Collections.Generic.List<Queries.Rule>>;
using UnityEditor.PackageManager;

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

namespace Queries
{
    sealed class RuleInterpreter
    {
        static readonly string[] Dividers = { Environment.NewLine, "\r\n", "\n" };

        const string TimeStamp = "timestamp";
        const string Rule = "rule";
        private static float TimeStampHash;
        private static float RuleHash;

        public RuleInterpreter()
        {
            TimeStampHash = TimeStamp.GetHashCode();
            RuleHash = RuleHash.GetHashCode();
        }

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

        private static IEnumerable<Criterion> ParseCriteriaCodes(int id, string criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                return Enumerable.Empty<Criterion>();
            }

            return criteria
                .Split(Dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretCriterion(id, x));
        }

        private static IEnumerable<Rememberer> ParseRemembererCodes(int id, string rememberers)
        {
            if (string.IsNullOrWhiteSpace(rememberers))
            {
                return Enumerable.Empty<Rememberer>();
            }

            return rememberers
                .Split(Dividers, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => InterpretRememberer(id, x));
        }

        // TODO: C# 8 Record type.
        sealed class RuleSplit
        {
            public StateSource Source { get; }
            public string Key { get; }
            public char Operator { get; }
            public float Value { get; }

            public RuleSplit(StateSource source, string key, char @operator, float value)
            {
                Source = source;
                Key = key;
                Operator = @operator;
                Value = value;
            }
        }

        static readonly Dictionary<char, StateSource> SourceCodes = new Dictionary<char, StateSource>()
        {
            { 'e', StateSource.Event },
            { 'c', StateSource.Character },
            { 'm', StateSource.Memory },
            { 'w', StateSource.World },
            { 't', StateSource.Target },
            //{ '@', StateSource.Custom }, // TODO
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

            // According to valve, basically all comparisons can be handled as an interval on a number line.
            // See: Optimization #6 (page 114) https://www.gdcvault.com/play/1015317/AI-driven-Dynamic-Dialog-through
            var valueString = raw.Substring(index + 1).ToLowerInvariant();
            float value;

            // Handle reserved words/characters, or hash strings
            {
                if (valueString == "timestamp")
                {
                    // TODO: This won't work
                    value = Time.time;
                }
                else  if (valueString == "rule")
                {
                    // Set the value equal to the rule's Id.
                    value = id;
                }
                else if (valueString.Contains('.') || valueString.All(char.IsDigit))
                {
                    // A number of some sort
                    value = float.Parse(valueString);
                }
                else
                {
                    // Assume this is some sort of string comparison. Hash the string.
                    value = valueString.GetHashCode();
                }
            }

            split = new RuleSplit(source, key, op, value);
            return true;
        }

        private static Criterion InterpretCriterion(int id, string code)
        {
            if (SplitOnOperator(id, code, out var split, '=', '>', '<', '!'))
            {
                var source = split.Source;
                var key = split.Key;
                var value = split.Value;
                var op = split.Operator;

                switch (op)
                {
                    case '=':
                        return new Criterion(key, source, value, value);
                    case '>':
                        return new Criterion(key, source, value, float.MaxValue);
                    case '<':
                        return new Criterion(key, source, float.MinValue, value);
                    case '!':
                        return new Criterion(key, source, value + float.Epsilon, value - float.Epsilon);
                }
            }

            // Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }

        private static Rememberer InterpretRememberer(int id, string code)
        {
            if (SplitOnOperator(id, code, out var split, '=', '-', '+', '*', '/'))
            {
                float value;

                if (split.Value == TimeStampHash)
                {
                    value = Time.time;
                }
                else if (split.Value == RuleHash)
                {
                    // Set the value equal to the rule's Id.
                    value = id;
                }
                else
                {
                    value = split.Value;
                }

                switch (split.Operator)
                {
                    case '=':
                        return new Set(split.Key, split.Source, value);
                    case '-':
                        return new Subtract(split.Key, split.Source, value);
                    case '+':
                        return new Add(split.Key, split.Source, value);
                    case '*':
                        return new Multiply(split.Key, split.Source, value);
                    case '/':
                        return new Divide(split.Key, split.Source, value);
                }
            }

            // Failed to interpret.
            Debug.LogError($"Failed to interpret criteria: {code}.");
            return null;
        }
    }
}