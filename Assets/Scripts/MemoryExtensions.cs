using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class GameObjectMemoryExtensions
    {
        /// <summary>
        /// Gets the memory container component on an object.
        /// </summary>
        public static IMemoryRetainer GetMemoryRetainer(this GameObject @this)
        {
            return @this.GetComponent<IMemoryRetainer>();
        }

        /// <summary>
        /// Gets the memory of an object.
        /// </summary>
        public static Dictionary<string, object> GetMemory(this GameObject @this)
        {
            return GetMemoryRetainer(@this)?.Memory;
        }
        
        /// <summary>
        /// Sets a memory value on a gameobject's memory container, if it has one.
        /// If it doesn't, one is added.
        /// </summary>
        public static void SetMemory(this GameObject @this, string key, object value)
        {
            var memory = GetMemory(@this) ?? @this.AddComponent<MemoryContainer>().Memory;
            memory[key] = value;
        }
    }

    public static class TransformMemoryExtensions
    {
        /// <summary>
        /// Gets the memory container component on a transform.
        /// </summary>
        public static IMemoryRetainer GetMemoryRetainer(this Transform @this)
        {
            return @this.GetComponent<IMemoryRetainer>();
        }

        /// <summary>
        /// Gets the memory of a transform.
        /// </summary>
        public static Dictionary<string, object> GetMemory(this Transform @this)
        {
            return GetMemoryRetainer(@this)?.Memory;
        }

        /// <summary>
        /// Sets a memory value on a transform's memory container, if it has one.
        /// If it doesn't, one is added.
        /// </summary>
        public static void SetMemory(this Transform @this, string key, object value)
        {
            var memory = GetMemory(@this) ?? @this.gameObject.AddComponent<MemoryContainer>().Memory;
            memory[key] = value;
        }
    }
}
