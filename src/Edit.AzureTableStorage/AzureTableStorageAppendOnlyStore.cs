using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public sealed class AzureTableStorageAppendOnlyStore : IStreamStore, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        private CloudStorageAccount _cloudStorageAccount;
        private string _tableName;
        private readonly IFramer _framer;
        private readonly IEntitiesReader _entitiesReader;

        /// <summary>
        /// When running the storage emulator the maximum storage per database row is less than in product
        /// </summary>
        public static bool IsStorageEmulator { get; set; }

        internal AzureTableStorageAppendOnlyStore(IFramer framer, IEntitiesReader entitiesReader)
        {
            _framer = framer;
            _entitiesReader = entitiesReader;
        }

        public static async Task<IStreamStore> CreateAsync(CloudStorageAccount cloudStorageAccount, string tableName,
                                                           ISerializer serializer)
        {
            return await CreateAsync(cloudStorageAccount, tableName, serializer, new MultipleRetrieveEntitiesReader());
        }

        public static async Task<IStreamStore> CreateAsync(CloudStorageAccount cloudStorageAccount, string tableName, ISerializer serializer, IEntitiesReader entitiesReader)
        {
            var streamStore = new AzureTableStorageAppendOnlyStore(new Framer(serializer), entitiesReader);
            await streamStore.StartAsync(cloudStorageAccount, tableName);
            return streamStore;
        }

        public static async Task<IStreamStore> CreateAsync(CloudStorageAccount cloudStorageAccount, string tableName, IFramer framer, IEntitiesReader entitiesReader)
        {
            var streamStore = new AzureTableStorageAppendOnlyStore(framer, entitiesReader);
            await streamStore.StartAsync(cloudStorageAccount, tableName);
            return streamStore;
        }

        private async Task StartAsync(CloudStorageAccount cloudStorageAccount, string tableName)
        {
            _cloudStorageAccount = cloudStorageAccount;
            _tableName = tableName;

            CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable cloudTable = cloudTableClient.GetTableReference(tableName);

            // create container if it does not exist on startup
            await cloudTable.CreateIfNotExistAsync();
        }

        #region WriteAsync

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, IStoredDataVersion expectedVersion)
        {
            await WriteAsync(streamName, chunks, Timeout.InfiniteTimeSpan, expectedVersion);
        }

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, TimeSpan timeout, IStoredDataVersion expectedVersion)
        {
            await WriteAsync(streamName, chunks, timeout, CancellationToken.None, expectedVersion);
        }

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, CancellationToken token, IStoredDataVersion expectedVersion)
        {
            await WriteAsync(streamName, chunks, Timeout.InfiniteTimeSpan, token, expectedVersion);
        }

        public Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, TimeSpan timeout,
                                     CancellationToken token, IStoredDataVersion expectedVersion)
        {
            AzureTableStorageEntryDataVersion azureDataVersion = null;
            bool isNewEntity = true;
            String version = null;
            if (expectedVersion != null)
            {
                azureDataVersion = expectedVersion as AzureTableStorageEntryDataVersion;
                if (azureDataVersion == null)
                {
                    Logger.Warn("Illegal version");
                    throw new ConcurrencyException(streamName, expectedVersion);
                }
                version = azureDataVersion.Version;
                isNewEntity = false; // If an expected version is available, this is an update of existing data
            }

            var updatedEntities = _framer.Write(chunks, azureDataVersion);
            ITableEntity firstEntity = null, secondEntity = null;
            int noEntities = 0;
            foreach (var entity in updatedEntities)
            {
                entity.PartitionKey = streamName;
                noEntities++;

                if (noEntities == 1)
                {
                    entity.ETag = version ?? "*"; // "*" means that it will overwrite it and discard optimistic concurrency                    
                    firstEntity = entity.Entity;
                }
                else if ((noEntities == 2 && isNewEntity) || noEntities > 2) // We do not allow more than one updated row and one new row. The limitation is on the batch updated on the Azure Table Storage. At most it can insert/update 4 MB of data per batch
                {
                    Logger.Warn("Attempt to save more than max amount of new data");
                    throw new StorageSizeException("Cannot write new data that produces more than one new table row (about 1MB of new data)");
                }
                else // If more than 1 updated rows, the first is an updated row and the second a new row
                {
                    entity.ETag = "*";
                    secondEntity = entity.Entity;
                }
            }

            if (noEntities == 1)
            {
                return WriteAsync(streamName, firstEntity, timeout, token, expectedVersion);
            }
            return WriteMultipleEntitiesAsync(firstEntity, secondEntity, timeout, token, expectedVersion);
        }

        private async Task WriteMultipleEntitiesAsync(ITableEntity entityToUpdate, ITableEntity entityToInsert, TimeSpan timeout, CancellationToken token, IStoredDataVersion expectedVersion)
        {
            var cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(_tableName);

            var tableBatch = new TableBatchOperation();
            tableBatch.Replace(entityToUpdate);
            tableBatch.InsertOrReplace(entityToInsert);

            try
            {
                await cloudTable.ExecuteBatchAsync(tableBatch);
            }
            catch (StorageException e)
            {
                Logger.Error("Error executing table batch " + e);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Error executing table batch " + ex);
                throw;
            }
        }

        private async Task WriteAsync(string streamName, ITableEntity entity, TimeSpan timeout, CancellationToken token, IStoredDataVersion expectedVersion)
        {
            var cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(_tableName);

            bool isMissing = false;

            try
            {
                await cloudTable.ReplaceAsync(entity);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409 || e.RequestInformation.HttpStatusCode == 412) // 409 == Conflict, 412 == Preconfition Failed
                {
                    Logger.DebugFormat("Storage concurrency exception {0}", e);
                    throw new ConcurrencyException(streamName, expectedVersion);
                }
                else if (e.RequestInformation.HttpStatusCode == 404)
                {
                    isMissing = true;
                }
                else
                {
                    throw;
                }
            }

            if (isMissing)
            {
                await InsertEmptyAsync(streamName, timeout, token, entity.RowKey);
                await WriteAsync(streamName, entity, timeout, token, expectedVersion);
            }
        }

        #endregion

        #region ReadAsync

        public async Task<ChunkSet> ReadAsync(string streamName)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout)
        {
            return await ReadAsync(streamName, timeout, CancellationToken.None);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, CancellationToken token)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan, token);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            var cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(_tableName);

            bool isMissing = false;

            try
            {
                var entities = await _entitiesReader.ReadRows(cloudTable, streamName);
                if (entities == null || entities.Count == 0)
                {
                    Logger.InfoFormat("No entity was found with stream name '{0}'", streamName);
                    isMissing = true;
                }
                else
                {
                    var chunks = _framer.Read<Chunk>(entities);
                    var lastEntity = entities.Last();
                    return new ChunkSet(chunks, new AzureTableStorageEntryDataVersion { Version = lastEntity.ETag, LastRowKey = int.Parse(lastEntity.RowKey), FirstChunkNoOfRow = lastEntity.FirstChunkNoWrittenToRow });
                }
            }
            catch (StorageException exception)
            {
                Logger.DebugFormat("ERROR: Exception {0} while retrieving cloud table entity async", exception);

                if (exception.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                isMissing = true;
            }

            if (isMissing)
            {
                //Logger.DebugFormat("BEGIN: Insert empty async: StreamName: '{0}'", streamName);
                //await InsertEmptyAsync(streamName, timeout, token);
                Logger.DebugFormat("Returning null");
                return null;
                //Logger.DebugFormat("END: Insert empty async: StreamName: '{0}'", streamName);
            }

            return await ReadAsync(streamName, timeout, token);
        }

        private async Task InsertEmptyAsync(string streamName, TimeSpan timeout, CancellationToken token, String rowKey)
        {
            var cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(_tableName);

            var entity = new AppendOnlyStoreDynamicTableEntity
            {
                ETag = "*",
                PartitionKey = streamName,
                RowKey = rowKey
            }.Entity;

            try
            {
                await cloudTable.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ERROR: Exception thrown while inserting empty on id {0}", ex, streamName);
            }
        }

        #endregion

        public void Dispose()
        {
        }
    }
}