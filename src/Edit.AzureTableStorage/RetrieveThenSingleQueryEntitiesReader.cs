using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NLog;

namespace Edit.AzureTableStorage
{
    public class RetrieveThenSingleQueryEntitiesReader : IEntitiesReader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<IList<AppendOnlyStoreDynamicTableEntity>> ReadRows(CloudTable cloudTable, string streamName)
        {
            var entities = new List<AppendOnlyStoreDynamicTableEntity>();

            Logger.Debug("BEGIN: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
            var entity = await cloudTable.RetrieveAsync(streamName, MultipleRowsDataEntityWriter.FirstRowKey.ToString());
            Logger.Debug("END: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);

            if (entity == null)
            {
                return entities;
            }

            AppendOnlyStoreDynamicTableEntity firstEntitiy = AppendOnlyStoreDynamicTableEntity.Parse(entity);
            entities.Add(firstEntitiy);
            if (firstEntitiy.IsFull)
            {
                Logger.Debug("BEGIN: Retrieve cloud table entities query async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
                var allRemainingEntities = await cloudTable.RetrieveMultipleAsync<DynamicTableEntity>(streamName,
                                                                                    entity.RowKey);
                Logger.Debug("END: Retrieve cloud table entities query async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
                entities.AddRange(allRemainingEntities.Select(AppendOnlyStoreDynamicTableEntity.Parse));
            }

            // The result is sorted by the String value of RowKey, so if the value gets larger than 1 char, a sorting must be performed
            if (entities.Count > 10)
            {
                entities.Sort(new EntitySort());
            }

            return entities;
        }

    }
}
