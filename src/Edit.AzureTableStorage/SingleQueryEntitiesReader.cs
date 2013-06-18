using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class SingleQueryEntitiesReader : IEntitiesReader
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public async Task<IList<AppendOnlyStoreDynamicTableEntity>> ReadRows(CloudTable cloudTable, string streamName)
        {
            var entities = new List<AppendOnlyStoreDynamicTableEntity>();

            Logger.DebugFormat("BEGIN: Retrieve all cloud table entities async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
            var dynEntities = await cloudTable.RetrieveMultipleAsync<DynamicTableEntity>(streamName);
            Logger.DebugFormat("END: Retrieve all cloud table entities async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);

            if (dynEntities == null)
            {
                return entities;
            }

            entities.AddRange(dynEntities.Select(AppendOnlyStoreDynamicTableEntity.Parse));

            // The result is sorted by the String value of RowKey, so if the value gets larger than 1 char, a sorting must be performed
            if (entities.Count > 10)
            {
                entities.Sort(new EntitySort());
            }

            return entities;
        }
    }
}
