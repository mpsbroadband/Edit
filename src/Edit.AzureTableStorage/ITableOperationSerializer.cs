using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public interface ITableOperationSerializer
    {
        TableBatchOperation Serialize<T>(string streamName, string sequencePrefix, IEnumerable<T> items, IEnumerable<DynamicTableEntity> existingEntities, bool developmentStorage) where T : class;
        IEnumerable<T> Deserialize<T>(IEnumerable<DynamicTableEntity> entities, string column, int position);
    }
}