using System.Collections.Generic;
using UnityEngine;

namespace Queries
{
    public interface IMemoryRetainer
    {
        Dictionary<string, object> Memory { get; }
    }

    /// <summary>
    /// Otherwise functionless component for giving a memory to a gameobject.
    /// </summary>
    public class MemoryContainer : MonoBehaviour, IMemoryRetainer
    {
        public Dictionary<string, object> Memory { get; } = new Dictionary<string, object>();
    }
}
