using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class TableStorageVersion : IVersion
    {
        public TableStorageVersion(IEnumerable<DynamicTableEntity> entities)
        {
            Entities = new ReadOnlyCollection<DynamicTableEntity>(entities.ToList());
            PartitionKey = Entities.Last().PartitionKey;
            RowKey = Entities.Last().RowKey;
            Column = Entities.Last().Properties.OrderBy(p => p.Key).Last().Key;
            Position = Entities.Last().Properties.OrderBy(p => p.Key).Last().Value.BinaryValue.Length;
        }

        public IEnumerable<DynamicTableEntity> Entities { get; private set; }
        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }
        public string Column { get; private set; }
        public int Position { get; private set; }

        public override string ToString()
        {
            return Entities.Select(e => e.ETag).LastOrDefault() ?? string.Empty;
        }
    }
}