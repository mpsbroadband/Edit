using System.Collections.Generic;

namespace Edit.AzureTableStorage
{
    internal class Framer : IFramer
    {
        private readonly IChunkSerializer _serializer;

        public Framer(ISerializer serializer)
        {
            _serializer = new ChunkSerializer(serializer);
        }

        public IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreDynamicTableEntity> entities) where T : class
        {
            var reader = new MultipleRowsDataEntityReader(_serializer);
            return reader.Read<T>(entities);
        }

        public IEnumerable<AppendOnlyStoreDynamicTableEntity> Write<T>(IEnumerable<T> frames, AzureTableStorageEntryDataVersion version) where T : class
        {
            var writer = new MultipleRowsDataEntityWriter(_serializer);
            return writer.GetUpdatedDataRows(frames, version);
        }

    }
}
