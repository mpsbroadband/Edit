using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class TableStorageStreamStore : IStreamStore
    {
        private readonly ITableOperationSerializer _serializer;
        private readonly CloudTableClient _client;
        private readonly CloudTable _table;
        private readonly bool _developmentStorage;

        public TableStorageStreamStore(ITableOperationSerializer serializer, CloudStorageAccount storageAccount)
        {
            _serializer = serializer;
            _client = storageAccount.CreateCloudTableClient();
            _table = _client.GetTableReference("EventStore");
            _table.CreateIfNotExists();

            _developmentStorage = _client.BaseUri == CloudStorageAccount.DevelopmentStorageAccount
                                                                        .CreateCloudTableClient()
                                                                        .BaseUri;
        }

        public async Task WriteAsync<T>(string streamName, IEnumerable<T> items, IVersion expectedVersion, CancellationToken token) where T : class 
        {
            var version = expectedVersion as TableStorageVersion;
            var existingEntities = version != null ? version.Entities : new DynamicTableEntity[0];
            var batch = _serializer.Serialize(streamName, items, existingEntities, _developmentStorage);

            try
            {
                await _table.ExecuteBatchAsync(batch);
            }
            catch (StorageException)
            {
                throw new ConcurrencyException(streamName, expectedVersion);
            }
        }

        public async Task<StreamSegment<T>> ReadAsync<T, TSnapshot>(string streamName, SnapshotEnvelope<TSnapshot> snapshot, CancellationToken cancellationToken) where T : class
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, streamName);

            if (snapshot != null)
            {
                filter = TableQuery.CombineFilters(filter, "and", TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, snapshot.RowKey));
            }

            var continuationToken = new TableContinuationToken();
            var entities = new List<DynamicTableEntity>();

            while (continuationToken != null)
            {
                var result = await _table.ExecuteQuerySegmentedAsync(
                                        new TableQuery {FilterString = filter}, continuationToken, cancellationToken);
                entities.AddRange(result.Results);
                continuationToken = result.ContinuationToken;
            }

            var items = _serializer.Deserialize<T>(
                                    entities, snapshot != null ? snapshot.Column : null, 
                                    snapshot != null ? snapshot.Position : 0);

            return new StreamSegment<T>(items, new TableStorageVersion(entities));
        }
    }
}