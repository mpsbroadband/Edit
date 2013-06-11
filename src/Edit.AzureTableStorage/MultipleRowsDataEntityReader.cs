using System.Collections.Generic;
using System.Linq;

namespace Edit.AzureTableStorage
{
    public class MultipleRowsDataEntityReader
    {
        private readonly IChunkSerializer _serializer;

        public MultipleRowsDataEntityReader(IChunkSerializer serializer)
        {
            _serializer = serializer;
        }

        private byte[] ReadChunksBlob(AppendOnlyStoreDynamicTableEntity entity)
        {
            var column = entity.CurrentColumn;
            var data = column.Get();
            while (column.ContainsMultipleColumnsChunk())
            {
                if (!entity.MoveNextColumn())
                {
                    throw new CorruptedStorageException("Could not read multiple columns chunk");
                }
                column = entity.CurrentColumn;
                var newData = column.Get();
                var buffer = new byte[data.Length + newData.Length];
                System.Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
                System.Buffer.BlockCopy(newData, 0, buffer, data.Length, newData.Length);
                data = buffer;
                if (column.IsLastPieceOfMultipleColumnsChunk())
                {
                    break;
                }
            }
            return data;
        }

        private IEnumerable<T> ReadColumns<T>(AppendOnlyStoreDynamicTableEntity entity) where T : class
        {
            var frames = new List<T>();
            while (entity.MoveNextColumn())
            {
                var data = ReadChunksBlob(entity);
                var columnFrames = ReadChunks<T>(data);
                int cnt = 0;
                foreach (var columnFrame in columnFrames)
                {
                    cnt++;
                    frames.Add(columnFrame);
                }
                if (cnt == 0)
                {
                    break;
                }
            }
            return frames;
        }

        private IEnumerable<T> ReadChunks<T>(byte[] data) where T : class
        {
            if (data != null)
            {
                return _serializer.Read<T>(data);
            }
            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreDynamicTableEntity> entities) where T : class
        {
            var frames = new List<T>();
            foreach (var appendOnlyStoreTableEntity in entities)
            {
                frames.AddRange(ReadColumns<T>(appendOnlyStoreTableEntity));
            }
            return frames;
        }
    }
}
