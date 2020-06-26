using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

sealed class Query
{
    /// <summary>
    /// Describes the type of triggering event. E.g: SeenObject, Hurt, Interact.
    /// </summary>
    public readonly string Concept;

    /// <summary>
    /// The origin of the query. Could be the triggering character's name, or a generic tag.
    /// </summary>
    public readonly string Who;

    private readonly Dictionary<string, object>[] stateSources;

    public Query(
        string concept,
        string who,
        Dictionary<string, object> eventState,
        Dictionary<string, object> character,
        Dictionary<string, object> memory,
        Dictionary<string, object> world)
    {
        Concept = concept;
        Who = who;

        stateSources = new Dictionary<string, object>[4];
        stateSources[0] = eventState;
        stateSources[1] = character;
        stateSources[2] = memory;
        stateSources[3] = world;
    }

    /// <summary>
    /// Check for a value with given type [T] in memory. 
    /// Will return default if no such key exists.
    /// </summary>
    public T Get<T>(string key, StateSource source)
    {
        var s = stateSources[(int)source];
        return s.ContainsKey(key)
           ? (T)s[key]
           : default;
    }

    /// <summary>
    /// Set a value in memory.
    /// A new value for the key will be created if it doesn't already exist.
    /// </summary>
    public void Set(string key, object value, StateSource source)
    {
        if (source == StateSource.Event || source == StateSource.Character)
        {
            Debug.LogError($"Cannot set values in source {source}.");
        }

        var s = stateSources[(int)source];
        s[key] = value;
    }

    /// <summary>
    /// Runs a method on the value at key, setting it back in memory.
    /// </summary>
    public void Transform<T>(string key, Func<T, T> transformation, StateSource source)
    {
        var value = Get<T>(key, source);
        Set(key, transformation.Invoke(value), source);
    }

    /// <summary>
    /// Increments an int stored at [key] by [amount].
    /// </summary>
    public void Increment(string key, int amount, StateSource source)
    {
        Set(key, Get<int>(key, source) + amount, source);
    }
}