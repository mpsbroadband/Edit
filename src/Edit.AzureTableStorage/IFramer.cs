using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal interface IFramer
    {
        IEnumerable<T> Read<T>(IEnumerable<AppendOnlyStoreTableEntity> entities) where T : class;
        IEnumerable<AppendOnlyStoreTableEntity> Write<T>(IEnumerable<T> frames, AzureTableStorageEntryDataVersion version) where T : class;
    }
}
