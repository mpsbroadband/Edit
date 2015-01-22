using System.Collections.Generic;

namespace Edit
{
    public sealed class StreamSegment<T>
    {
        public IVersion Version { get; private set; }
        public IEnumerable<T> StreamItems { get; private set; }
        public IEnumerable<T> CausationItems { get; private set; }

        public StreamSegment(IEnumerable<T> streamItems, IEnumerable<T> causationItems, IVersion version)
        {
            StreamItems = new List<T>(streamItems);
            CausationItems = causationItems;
            Version = version;
        }
    }
}