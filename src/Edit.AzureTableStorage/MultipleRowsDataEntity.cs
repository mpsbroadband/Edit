using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Edit.AzureTableStorage
{
    internal class MultipleRowsDataEntity
    {
        private readonly IChunkSerializer _serializer;

        public const int FirstRowKey = 0;

        public MultipleRowsDataEntity(IChunkSerializer serializer)
        {
            _serializer = serializer;
        }

        private int FastForwardToFirstChunkInLastWrittenRow<T>(int currChunkNo, int firstChunkOfRow, IEnumerator<T> chunkEnum) where T : class
        {
            while (currChunkNo < firstChunkOfRow)
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

        private IEnumerable<AppendOnlyStoreTableEntity> WriteChunksToEntity<T>(IEnumerable<T> chunks, AzureTableStorageEntryDataVersion version = null) where T : class
        {
            var entities = new List<AppendOnlyStoreTableEntity>();
            var chunkEnum = chunks.GetEnumerator();
            int currChunkNo = 0;
            int currRowNo = FirstRowKey;
            if (version != null)
            {
                currRowNo = version.LastRowKey;
                currChunkNo = FastForwardToFirstChunkInLastWrittenRow(currChunkNo, version.FirstChunkNoOfRow, chunkEnum);
            }
            var currentRow = new DataRow(currChunkNo, currRowNo);
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
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                currentRow.Dispose();
            }
            return entities;
        }

        public IEnumerable<AppendOnlyStoreTableEntity> GetDataRows<T>(IEnumerable<T> chunks) where T : class
        {
            return WriteChunksToEntity(chunks);
        }

        public IEnumerable<AppendOnlyStoreTableEntity> GetUpdatedDataRows<T>(IEnumerable<T> chunks, AzureTableStorageEntryDataVersion version) where T : class
        {
            return WriteChunksToEntity(chunks, version);
        }

        public IEnumerable<T> GetChunks<T>(IEnumerable<AppendOnlyStoreTableEntity> tableRows) where T : class
        {
            return null;
        }
    }
}
