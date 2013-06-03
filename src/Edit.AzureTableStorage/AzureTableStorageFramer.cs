using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    public class AzureTableStorageFramer : IFramer
    {
        private readonly ChunkSerializer _serializer;

        public AzureTableStorageFramer(ISerializer serializer)
        {
            _serializer = new ChunkSerializer(serializer);
        }

        private IEnumerable<T> ReadColumns<T>(IEnumerable<DataColumn> columns) where T : class
        {
            var frames = new List<T>();
            foreach (var column in columns)
            {
                var columnFrames = ReadColumn<T>(column);
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

        private IEnumerable<T> ReadColumn<T>(DataColumn column) where T : class
        {
            byte[] data = column.Get();
            if (data != null)
            {
                return _serializer.Read<T>(data);
            }
            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> Read<T>(AppendOnlyStoreTableEntity entity) where T : class
        {
            var columns = new ColumnsWrapper(entity);
            return ReadColumns<T>(columns.DataColumns);
        }

        public AppendOnlyStoreTableEntity Write<T>(IEnumerable<T> frames) where T : class
        {
            var entity = new AppendOnlyStoreTableEntity();
            var columns = new ColumnsWrapper(entity).DataColumns.GetEnumerator();
            var memoryStream = new MemoryStream();

            try
            {
                int currSize = 0;
                int noChunksInColumn = 0;
                columns.MoveNext();
                DataColumn column = columns.Current;
                foreach (var frame in frames)
                {
                    var result = _serializer.Write(frame);
                    int resultSize = result.Length;
                    if (resultSize > DataColumn.MaxSize)
                        // Cannot handle single message being larger than one column. Could be fixed by allowing a message to expand to multiple columns and rows
                    {
                        throw new StorageSizeException("Messages larger than " + DataColumn.MaxSize +
                                                       " bytes is not supported");
                    }
                    currSize += resultSize;
                    if (currSize > DataColumn.MaxSize)
                    {
                        column.Set(memoryStream.ToArray());
                        column.SetNumberOfChunks(noChunksInColumn);
                        if (!columns.MoveNext())
                        {
                            throw new StorageSizeException(
                                "Aggregates larger than one row is not supported yet. Multi row support is coming.");
                        }
                        column = columns.Current;
                        memoryStream.Dispose();
                        memoryStream = new MemoryStream();
                        currSize = resultSize;
                        noChunksInColumn = 1;
                    }
                    else
                    {
                        noChunksInColumn++;
                    }
                    memoryStream.Write(result, 0, result.Length);
                }

                column.Set(memoryStream.ToArray());
                column.SetNumberOfChunks(noChunksInColumn);
            }
            finally
            {
                memoryStream.Dispose();
            }
            return entity;
        }

    }
}
