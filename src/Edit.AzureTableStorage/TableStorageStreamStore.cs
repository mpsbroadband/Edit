using System;
using System.Collections.Generic;
using System.Linq;
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
            _table = _client.GetTableReference("StreamStore");
            _table.CreateIfNotExists();

            _developmentStorage = _client.BaseUri == CloudStorageAccount.DevelopmentStorageAccount
                                                                        .CreateCloudTableClient()
                                                                        .BaseUri;
        }

        public async Task<IVersion> WriteAsync<T>(string streamName, IEnumerable<T> items, IVersion expectedVersion, CancellationToken token) where T : class 
        {
            var version = expectedVersion as TableStorageVersion;
            var existingEntities = version != null ? version.Entities : new DynamicTableEntity[0];
            var batch = _serializer.Serialize(streamName, items, existingEntities, _developmentStorage);

            try
            {
                var result = await _table.ExecuteBatchAsync(batch);

                return new TableStorageVersion(result.Select(r => (DynamicTableEntity) r.Result));
            }
            catch (StorageException)
            {
                throw new ConcurrencyException(streamName, expectedVersion);
            }
        }

        public async Task<StreamSegment<T>> ReadAsync<T, TSnapshot>(string streamName, ISnapshotEnvelope<TSnapshot> snapshot, CancellationToken cancellationToken) where T : class
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, streamName);
            var tableSnapshot = snapshot as TableStorageSnapshotEnvelope<TSnapshot>;

            if (tableSnapshot != null)
            {
                filter = TableQuery.CombineFilters(filter, "and", TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, tableSnapshot.RowKey));
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
                                    entities, tableSnapshot != null ? tableSnapshot.Column : null,
                                    tableSnapshot != null ? tableSnapshot.Position : 0);

            return new StreamSegment<T>(items, new TableStorageVersion(entities));
        }
    }
}