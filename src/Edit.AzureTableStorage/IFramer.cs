using System.Collections.Generic;

namespace Edit.AzureTableStorage
{
    internal interface IFramer
    {
        IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreDynamicTableEntity> entities) where T : class;
        IEnumerable<AppendOnlyStoreDynamicTableEntity> Write<T>(IEnumerable<T> frames, AzureTableStorageEntryDataVersion version) where T : class;
    }
}
