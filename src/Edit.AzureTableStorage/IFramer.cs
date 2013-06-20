using System.Collections.Generic;

namespace Edit.AzureTableStorage
{
    public interface IFramer
    {
        IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreDynamicTableEntity> entities) where T : class;
        IEnumerable<AppendOnlyStoreDynamicTableEntity> Write<T>(IEnumerable<T> frames, IStoredDataVersion version) where T : class;
    }
}
