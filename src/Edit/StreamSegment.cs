using System.Collections.Generic;

namespace Edit
{
    public sealed class StreamSegment<T>
    {
        public IVersion Version { get; private set; }
        public IEnumerable<T> Items { get; private set; }

        public StreamSegment(IEnumerable<T> items, IVersion version)
        {
            Items = new List<T>(items);
            Version = version;
        }
    }
}