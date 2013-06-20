using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class MultipleRowsDataEntityWriter
    {
        private readonly IChunkSerializer _serializer;

        public const int FirstRowKey = 0;

        public MultipleRowsDataEntityWriter(IChunkSerializer serializer)
        {
            _serializer = serializer;
        }

        private int FastForwardToFirstChunkInLastWrittenRow<T>(int currChunkNo, int firstChunkOfRow, IEnumerator<T> chunkEnum) where T : class
        {
            while (currChunkNo < (firstChunkOfRow-1))
            {
                if (chunkEnum.MoveNext())
                {
                    currChunkNo++;
                }
                else
                {
                    throw new StorageSizeException("Store is append only - cannot reduce the size of a stored entity");
                }
            }
            return currChunkNo;
        }

        private IEnumerable<AppendOnlyStoreDynamicTableEntity> WriteChunksToEntity<T>(IEnumerable<T> chunks, AzureTableStorageEntryDataVersion version = null) where T : class
        {
            var entities = new List<AppendOnlyStoreDynamicTableEntity>();
            var chunkEnum = chunks.GetEnumerator();
            int currChunkNo = 0;
            int currRowNo = FirstRowKey;
            int firstChunkNo = currChunkNo;
            if (version != null)
            {
                currRowNo = version.LastRowKey;
                currChunkNo = FastForwardToFirstChunkInLastWrittenRow(currChunkNo, version.FirstChunkNoOfRow, chunkEnum);
                firstChunkNo = version.FirstChunkNoOfRow;
            }
            var currentRow = new DataRow(firstChunkNo, currRowNo);
            try
            {
                while (chunkEnum.MoveNext())
                {
                    currChunkNo++;
                    var chunk = chunkEnum.Current;
                    var result = _serializer.Write(chunk);
                    if (!currentRow.WriteChunk(result))
                    {
                        entities.Add(currentRow.CreateEntity());
                        currRowNo++;
                        currentRow.Dispose();
                        currentRow = new DataRow(currChunkNo, currRowNo);
                        currentRow.WriteChunk(result);
                    }
                }
                entities.Add(currentRow.CreateEntity());
            }
            finally
            {
                currentRow.Dispose();
            }
            return entities;
        }

        public IEnumerable<AppendOnlyStoreDynamicTableEntity> GetDataRows<T>(IEnumerable<T> chunks) where T : class
        {
            return WriteChunksToEntity(chunks);
        }

        public IEnumerable<AppendOnlyStoreDynamicTableEntity> GetUpdatedDataRows<T>(IEnumerable<T> chunks, IStoredDataVersion version) where T : class
        {
            return WriteChunksToEntity(chunks, version as AzureTableStorageEntryDataVersion);
        }

        public IEnumerable<T> GetChunks<T>(IEnumerable<ITableEntity> tableRows) where T : class
        {
            return null;
        }
    }
}
