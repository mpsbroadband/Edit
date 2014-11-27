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
        }

        public IEnumerable<DynamicTableEntity> Entities { get; private set; }

        public override string ToString()
        {
            return Entities.Select(e => e.ETag).LastOrDefault() ?? string.Empty;
        }
    }
}