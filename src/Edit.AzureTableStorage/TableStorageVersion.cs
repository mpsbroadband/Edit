using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class TableStorageVersion : IVersion
    {
        public IEnumerable<DynamicTableEntity> Entities { get; private set; }
    }
}