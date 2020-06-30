using Assets.Scripts;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Queries
{
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

        /// <summary>
        /// State set by whatever event triggered a query. 
        /// Includes information about what is currently happening.
        /// </summary>
        private readonly Dictionary<string, float> @event;

        /// <summary>
        /// Includes information about the querying character's state, 
        /// such as that character's current position, rotation, stats, etc.
        /// </summary>
        private readonly Dictionary<string, float> character;

        /// <summary>
        /// The triggering character's personal memory.
        /// Intended to store running tallies of things seen, interacted with, performed, etc.
        /// </summary>
        private readonly Dictionary<string, float> memory;

        /// <summary>
        /// Collective memory of the gameworld.
        /// A globally accessible equivalent to [memory].
        /// </summary>
        private readonly Dictionary<string, float> world;

        public Query(
            QueryArgs args,
            Dictionary<string, float> world)
        {
            Concept = args.Concept;
            Who = args.Who;
            @event = args.Event;
            character = args.Character;
            memory = args.Memory;
            this.world = world;
        }

        /// <summary>
        /// Returns true if a value of type [T] exists in state, populating it into [result].
        /// [result] will return the default value if not present or of mismatching type.
        /// IMPORTANT: If the value is not present, the default value _will be set into state_.
        /// </summary>
        public bool Get(string key, StateSource source, out float result)
        {
            GetState(source).TryGetValue(key, out float value);

            if (source != StateSource.Event && source != StateSource.Character)
            {
                // TODO
                // Set the value into memory
                result = default;
                Set(key, result, source);
                return false;
            }

            result = value;
            return true;
        }

        /// <summary>
        /// Set a value in memory.
        /// A new value for the key will be created if it doesn't already exist.
        /// </summary>
        public void Set(string key, float value, StateSource source)
        {
            if (source == StateSource.Event || source == StateSource.Character)
            {
                Debug.LogError($"Cannot set values in source {source}.");
            }

            var state = GetState(source);
            state[key] = value;
        }

        private Dictionary<string, float> GetState(StateSource source)
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
                case StateSource.Target:
                    return GetTargetMemory();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Queries the reserved "Target" state for a gameobject, returning its memory.
        /// This will add memory to any valid gameobject returned that does not have one.
        /// </summary>
        private Dictionary<string, float> GetTargetMemory()
        {
            //if (Get<GameObject>("Target", StateSource.Event, out var obj) && obj != null)
            //{
            //    var mem = obj.GetMemory();
            //    if (mem == null)
            //    {
            //        mem = obj.AddComponent<MemoryContainer>().Memory;
            //    }
            //    return mem;
            //}
            //else
            //{
            //    return null;
            //}
        
            // TODO: Make this work
        }
    }
}