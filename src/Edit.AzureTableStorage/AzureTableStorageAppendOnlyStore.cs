using System;
using System.Collections.Generic;
using System.IO;
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

        public const int MaxColumnSize = 65355;

        public AzureTableStorageAppendOnlyStore(IFramer framer)
        {
            _framer = framer;
        }

        private const string RowKey = "0";

        public static async Task<IStreamStore> CreateAsync(CloudStorageAccount cloudStorageAccount, string tableName, ISerializer serializer)
        {
            var streamStore = new AzureTableStorageAppendOnlyStore(new AzureTableStorageFramer(serializer));
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
            var entity = _framer.Write(chunks);
            entity.PartitionKey = streamName;
            entity.RowKey = RowKey;

            String version = null;
            if (expectedVersion != null)
            {
                var azureDataVersion = expectedVersion as AzureTableStorageEntryDataVersion;
                if (azureDataVersion == null)
                {
                    throw new ConcurrencyException(streamName, expectedVersion);
                }
                version = azureDataVersion.Version;
            }
            entity.ETag = version ?? "*"; // "*" means that it will overwrite it and discard optimistic concurrency

            return WriteAsync(streamName, entity, timeout, token, expectedVersion);
        }

        private async Task WriteAsync(string streamName, AppendOnlyStoreTableEntity entity, TimeSpan timeout, CancellationToken token, IStoredDataVersion expectedVersion)
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
                if (e.RequestInformation.HttpStatusCode == 409 || e.RequestInformation.HttpStatusCode == 412) // 409 == Conflict
                {
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
                await InsertEmptyAsync(streamName, timeout, token);
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
                Logger.DebugFormat("BEGIN: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
                var entity = await cloudTable.RetrieveAsync<AppendOnlyStoreTableEntity>(streamName, RowKey);
                Logger.DebugFormat("END: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);

                if (entity == null)
                {
                    Logger.InfoFormat("No entity was found with stream name '{0}'", streamName);
                    isMissing = true;
                }
                else
                {
                    var chunks = _framer.Read<Chunk>(entity);
                    return new ChunkSet(chunks, new AzureTableStorageEntryDataVersion { Version = entity.ETag, LastRowKey = "0", IdOfFirstDataInLastRow = null });
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

        private async Task InsertEmptyAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            var cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            var cloudTable = cloudTableClient.GetTableReference(_tableName);

            var entity = new AppendOnlyStoreTableEntity()
            {
                ETag = "*",
                PartitionKey = streamName,
                RowKey = RowKey,
                Data = new byte[0]
            };

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