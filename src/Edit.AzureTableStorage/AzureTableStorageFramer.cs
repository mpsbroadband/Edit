using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal class AzureTableStorageFramer : IFramer
    {
        private readonly IChunkSerializer _serializer;

        public AzureTableStorageFramer(ISerializer serializer)
        {
            _serializer = new ChunkSerializer(serializer);
        }

        private byte[] ReadChunksBlob(IEnumerator<DataColumn> columnsEnum)
        {
            var column = columnsEnum.Current;
            var data = column.Get();
            while (column.ContainsMultipleColumnsChunk())
            {
                if (!columnsEnum.MoveNext())
                {
                    throw new CorruptedStorageException("Could not read multiple columns chunk");
                }
                column = columnsEnum.Current;
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

        private IEnumerable<T> ReadColumns<T>(IEnumerable<DataColumn> columns) where T : class
        {
            var frames = new List<T>();
            var columnsEnum = columns.GetEnumerator();
            while (columnsEnum.MoveNext())
            {
                var data = ReadChunksBlob(columnsEnum);
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

        public IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreTableEntity> entities) where T : class
        {
            List<T> frames = new List<T>();
            foreach (var appendOnlyStoreTableEntity in entities)
            {
                var columns = new ColumnsWrapper(appendOnlyStoreTableEntity);
                frames.AddRange(ReadColumns<T>(columns.DataColumns));
            }
            return frames;
        }

        public IEnumerable<AppendOnlyStoreTableEntity> Write<T>(IEnumerable<T> frames, AzureTableStorageEntryDataVersion version) where T : class
        {
            MultipleRowsDataEntity multiRowsEntity = new MultipleRowsDataEntity(_serializer);
            return multiRowsEntity.GetUpdatedDataRows(frames, version);
        }

    }
}
