using System.Collections.Generic;
using Edit.AzureTableStorage;

namespace Edit.PerformanceTests
{
    public class DummyReadFramer : IFramer
    {
        private readonly IChunkSerializer _serializer;

        public DummyReadFramer(ISerializer serializer)
        {
            _serializer = new ChunkSerializer(serializer);
        }

        public IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreDynamicTableEntity> entities) where T : class
        {
            return new List<T>();
        }

        public IEnumerable<AppendOnlyStoreDynamicTableEntity> Write<T>(IEnumerable<T> frames, IVersion version) where T : class
        {
            var writer = new MultipleRowsDataEntityWriter(_serializer);
            return writer.GetUpdatedDataRows(frames, version);
        }
    }
}
