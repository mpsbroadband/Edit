using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public interface IEntitiesReader
    {
        Task<IList<AppendOnlyStoreDynamicTableEntity>> ReadRows(CloudTable cloudTable, String streamName);
    }
}
