using System.Collections.Generic;

namespace Edit
{
    public sealed class ChunkSet
    {
        public string Version { get; private set; }
        public IEnumerable<Chunk> Chunks { get; private set; }

        public ChunkSet(IEnumerable<Chunk> chunks, string version)
        {
            Chunks = new List<Chunk>(chunks);
            Version = version;
        }
    }
}