using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal static class CloudTableExtensions
    {
        public static async Task<bool> CreateIfNotExistAsync(this CloudTable cloudTable)
        {
            return await Task.Factory.FromAsync<bool>(cloudTable.BeginCreateIfNotExists, cloudTable.EndCreateIfNotExists, null);
        }

        public static async Task<TableResult> ExecuteAsync(this CloudTable cloudTable, TableOperation tableOperation)
        {
            return
                await
                Task<TableResult>.Factory.FromAsync(cloudTable.BeginExecute, cloudTable.EndExecute, tableOperation, null);
        }

        public static async Task<IList<TableResult>> ExecuteBatchAsync(this CloudTable cloudTable, TableBatchOperation tableBatchOperation)
        {
            return
                await
                Task<IList<TableResult>>.Factory.FromAsync(cloudTable.BeginExecuteBatch, cloudTable.EndExecuteBatch, tableBatchOperation, null);
        }

        public static async Task<T> RetrieveAsync<T>(this CloudTable cloudTable, string partitionKey, string rowKey) where T : class, ITableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var task = await cloudTable.ExecuteAsync(retrieveOperation);
            return task.Result as T;
        }

        public static async Task<ITableEntity> RetrieveAsync(this CloudTable cloudTable, string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey);
            var task = await cloudTable.ExecuteAsync(retrieveOperation);
            return task.Result as ITableEntity;
        }

        public static async Task InsertAsync<T>(this CloudTable cloudTable, T tableEntity) where T : class, ITableEntity
        {
            var insertOperation = TableOperation.Insert(tableEntity);
            await cloudTable.ExecuteAsync(insertOperation);
        }

        public static async Task ReplaceAsync<T>(this CloudTable cloudTable, T tableEntity) where T : class, ITableEntity
        {
            var insertOperation = TableOperation.Replace(tableEntity);
            await cloudTable.ExecuteAsync(insertOperation);
        }

        public static async Task<IList<T>> RetrieveMultipleAsync<T>(this CloudTable cloudTable, string partitionKey, string excludeRowKey = null) where T : ITableEntity, new()
        {
            TableQuery<T> query;
            if (excludeRowKey != null)
            {
                query = new TableQuery<T>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, excludeRowKey)));
            }
            else
            {
                query = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)
                );                
            }

            return await ExecuteQueryAsync(cloudTable, query);
        }

        public static async Task<IList<T>> ExecuteQueryAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            CancellationToken ct = default(CancellationToken),
            Action<IList<T>> onProgress = null)
            where T : ITableEntity, new()
        {
            var items = new List<T>();

            TableContinuationToken token = null;

            do
            {
                TableQuerySegment<T> seg = await table.ExecuteQueryAsync(query, token, ct);
                token = seg.ContinuationToken;
                items.AddRange(seg);
                if (onProgress != null)
                    onProgress(items);
            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }

        public static Task<TableQuerySegment<T>> ExecuteQueryAsync<T>(
            this CloudTable table,
            TableQuery<T> query,
            TableContinuationToken token,
            CancellationToken ct = default(CancellationToken))
            where T : ITableEntity, new()
        {
            ICancellableAsyncResult ar = table.BeginExecuteQuerySegmented(query, token, null, null);
            ct.Register(ar.Cancel);

            return Task.Factory.FromAsync<TableQuerySegment<T>>(ar, table.EndExecuteQuerySegmented<T>);
        }

    }
}