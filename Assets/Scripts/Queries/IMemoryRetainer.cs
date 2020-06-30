using System.Collections.Generic;

namespace Queries
{
    public interface IMemoryRetainer
    {
        Dictionary<string, object> Memory { get; }
    }
}
