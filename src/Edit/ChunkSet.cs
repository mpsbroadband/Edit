using System.Collections.Generic;

namespace Edit
{
    public sealed class ChunkSet
    {
        public string Version { get; set; }
        public IEnumerable<Chunk> Chunks { get; set; }

        public ChunkSet(IEnumerable<Chunk> chunks, string version)
        {
            Chunks = new List<Chunk>(chunks);
            Version = version;
        }
    }
}