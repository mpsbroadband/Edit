using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureApi.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class TableStorageStreamStore : IStreamStore
    {
        internal const string StreamSequencePrefix = "stream";
        internal const string CausationSequencePrefix = "causation";

        private readonly ITableOperationSerializer _serializer;
        private readonly CloudTable _table;
        private readonly bool _developmentStorage;

        private async Task<CloudTable> GetTable()
        {
            await _table.CreateIfNotExistsAsync();
            return _table;
        }

        public TableStorageStreamStore(ITableOperationSerializer serializer, CloudStorageAccount storageAccount, string tableName = "StreamStore")
        {
            _serializer = serializer;
            var client = storageAccount.CreateCloudTableClient();
            _table = client.GetTableReference(tableName);

            _developmentStorage = client.BaseUri == CloudStorageAccount.DevelopmentStorageAccount
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
                var table = await GetTable();

                var result = await table.ExecuteBatchAsync(batch, token);

                return
                    new TableStorageVersion(streamName,
                        result.Select(r => (DynamicTableEntity) r.Result)
                            .Where(e => e.RowKey.StartsWith(StreamSequencePrefix)).OrderByAlphaNumeric(e => e.RowKey));
            }
            catch (StorageException e)
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

            var table = await GetTable();

            if (tableSnapshot != null)
            {
                var index = Convert.ToInt64(tableSnapshot.RowKey.Split('-')[1]);
                var count = 0;
                const int lap = 10;

                do
                {
                    var streamContinuationToken = new TableContinuationToken();
                    var filterCondition = string.Empty;
                    for (var i = index; i < index + lap; i++)
                    {
                        filterCondition += TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                            BatchOperationRow.FormatRowKey(StreamSequencePrefix, i)) + (i == index + lap - 1 ? "" : " or ");
                    }
                    index += lap;
                    streamFilter = TableQuery.CombineFilters(
                                             TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, streamName),
                                             "and", filterCondition);
                    while (streamContinuationToken != null)
                    {
                        var result = await table.ExecuteQuerySegmentedAsync(
                                                new TableQuery { FilterString = streamFilter }, streamContinuationToken, cancellationToken);
                        entities.AddRange(result.Results);
                        count = result.Results.Count;
                        streamContinuationToken = result.ContinuationToken;
                    }

                } while (count > 0);

                while (continuationToken != null)
                {
                    var result = await table.ExecuteQuerySegmentedAsync(
                                            new TableQuery { FilterString = causationFilter }, continuationToken, cancellationToken);
                    entities.AddRange(result.Results);
                    continuationToken = result.ContinuationToken;
                }
            }
            else
                while (continuationToken != null)
                {
                    var result = await table.ExecuteQuerySegmentedAsync(
                                            new TableQuery { FilterString = filter }, continuationToken, cancellationToken);
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