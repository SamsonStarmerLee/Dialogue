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

    private readonly Dictionary<string, object> @event;
    private readonly Dictionary<string, object> character;
    private readonly Dictionary<string, object> memory;
    private readonly Dictionary<string, object> world;

    public Query(
        string concept,
        string who,
        Dictionary<string, object> @event,
        Dictionary<string, object> character,
        Dictionary<string, object> memory,
        Dictionary<string, object> world)
    {
        Concept = concept;
        Who = who;
        this.@event = @event;
        this.character = character;
        this.memory = memory;
        this.world = world;
    }

    /// <summary>
    /// Returns true if a value of type [T] exists in state, populating it into [result].
    /// [result] will return the default value if not present or of mismatching type.
    /// IMPORTANT: If the value is not present, the default value _will be set into state_.
    /// </summary>
    public bool Get<T>(string key, StateSource source, out T result)
    {
        GetState(source).TryGetValue(key, out var value);

        if (value is T t)
        {
            result = t;
            return true;
        }
        else
        {
            result = default;
            Set(key, result, source);
            return false;
        }
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

        var state = GetState(source);
        state[key] = value;
    }

    /// <summary>
    /// Runs a method on the value at [key], setting it back in state.
    /// The transformation will be ran on a new default value if not present in state.
    /// </summary>
    public void Transform<T>(string key, Func<T, T> transformation, StateSource source)
    {
        Get(key, source, out T value);
        Set(key, transformation.Invoke(value), source);
    }

    /// <summary>
    /// Increments an int stored at [key] by [amount].
    /// Will increment and set a new value from 0 if not present in state.
    /// </summary>
    public void Increment(string key, int amount, StateSource source)
    {
        Get(key, source, out int value);
        Set(key, value + amount, source);
    }

    private Dictionary<string, object> GetState(StateSource source)
    {
        switch (source)
        {
            case StateSource.Event:
                return @event;
            case StateSource.Character:
                return character;
            case StateSource.Memory:
                return memory;
            case StateSource.World:
                return world;
            default:
                return null;
        }
    }
}