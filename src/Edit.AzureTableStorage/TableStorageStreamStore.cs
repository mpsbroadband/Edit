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
        internal const string StreamSequencePrefix = "stream";
        internal const string CausationSequencePrefix = "causation";

        private readonly ITableOperationSerializer _serializer;
        private readonly CloudTableClient _client;
        private readonly CloudTable _table;
        private readonly bool _developmentStorage;

        public TableStorageStreamStore(ITableOperationSerializer serializer, CloudStorageAccount storageAccount, string tableName = "StreamStore")
        {
            _serializer = serializer;
            _client = storageAccount.CreateCloudTableClient();
            _table = _client.GetTableReference(tableName);
            _table.CreateIfNotExists();

            _developmentStorage = _client.BaseUri == CloudStorageAccount.DevelopmentStorageAccount
                                                                        .CreateCloudTableClient()
                                                                        .BaseUri;
        }

        public async Task<IVersion> WriteAsync<T>(string streamName, string causationId, IEnumerable<T> items, IVersion expectedVersion, CancellationToken token) where T : class 
        {
            var version = expectedVersion as TableStorageVersion;
            var existingEntities = version != null ? version.Entities : new DynamicTableEntity[0];
            var itemsList = items.ToList();
            var streamBatch = _serializer.Serialize(streamName, StreamSequencePrefix, itemsList, 
                                                    existingEntities, _developmentStorage);
            var causationBatch = _serializer.Serialize(streamName, string.Concat(CausationSequencePrefix, "-", causationId), 
                                                       itemsList, new DynamicTableEntity[0], _developmentStorage);
            
            var batch = new TableBatchOperation();
            foreach (var operation in streamBatch.Union(causationBatch))
            {
                batch.Add(operation);
            }

            try
            {
                var result = await _table.ExecuteBatchAsync(batch, token);

                return
                    new TableStorageVersion(streamName,
                        result.Select(r => (DynamicTableEntity) r.Result)
                            .Where(e => e.RowKey.StartsWith(StreamSequencePrefix)).OrderByAlphaNumeric(e => e.RowKey));
            }
            catch (StorageException)
            {
                throw new ConcurrencyException(streamName, expectedVersion);
            }
        }

        public async Task<StreamSegment<T>> ReadAsync   <T, TSnapshot>(string streamName, string causationId, ISnapshotEnvelope<TSnapshot> snapshot, CancellationToken cancellationToken) where T : class
        {
            var tableSnapshot = snapshot as TableStorageSnapshotEnvelope<TSnapshot>;
            var streamRowKey = tableSnapshot != null
                ? tableSnapshot.RowKey
                : BatchOperationRow.FormatRowKey(StreamSequencePrefix, 0);
            var streamFilter = TableQuery.CombineFilters(
                                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, streamName), 
                                        "and", 
                                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, streamRowKey));
            var causationRowKeyStart = BatchOperationRow.FormatRowKey(string.Concat(CausationSequencePrefix, "-", causationId), 0);
            var causationRowKeyEnd = BatchOperationRow.FormatRowKey(string.Concat(CausationSequencePrefix, "-", causationId), 1000);
            var causationFilter = TableQuery.CombineFilters(
                                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, streamName),
                                        "and",
                                        TableQuery.CombineFilters(
                                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, causationRowKeyStart),
                                            "and",
                                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, causationRowKeyEnd)));
            var filter = TableQuery.CombineFilters(streamFilter, "or", causationFilter);
            var continuationToken = new TableContinuationToken();
            var entities = new List<DynamicTableEntity>();

            while (continuationToken != null)
            {
                var result = await _table.ExecuteQuerySegmentedAsync(
                                        new TableQuery {FilterString = filter}, continuationToken, cancellationToken);
                entities.AddRange(result.Results);
                continuationToken = result.ContinuationToken;
            }

            entities = entities.OrderByAlphaNumeric(e => e.RowKey).ToList();

            var streamItems = _serializer.Deserialize<T>(
                                    entities.Where(e => e.RowKey.StartsWith(StreamSequencePrefix)), 
                                    tableSnapshot != null ? tableSnapshot.Column : null,
                                    tableSnapshot != null ? tableSnapshot.Position : 0);

            var causationItems = _serializer.Deserialize<T>(
                                    entities.Where(e => e.RowKey.StartsWith(CausationSequencePrefix)), null, 0);

            return new StreamSegment<T>(streamItems, causationItems,
                new TableStorageVersion(streamName, entities.Where(e => e.RowKey.StartsWith(StreamSequencePrefix))));
        }
    }
}