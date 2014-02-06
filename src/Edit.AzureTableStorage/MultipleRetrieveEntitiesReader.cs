﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NLog;

namespace Edit.AzureTableStorage
{
    public class MultipleRetrieveEntitiesReader : IEntitiesReader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<IList<AppendOnlyStoreDynamicTableEntity>> ReadRows(CloudTable cloudTable, string streamName)
        {
            var entities = new List<AppendOnlyStoreDynamicTableEntity>();
            int currRowKey = MultipleRowsDataEntityWriter.FirstRowKey;

            Logger.Debug("BEGIN: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
            var entity = await cloudTable.RetrieveAsync(streamName, currRowKey.ToString());
            Logger.Debug("END: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);

            if (entity == null)
            {
                return entities;
            }

            AppendOnlyStoreDynamicTableEntity lastEntity = AppendOnlyStoreDynamicTableEntity.Parse(entity);
            entities.Add(lastEntity);
            if (lastEntity.IsFull)
            {
                while (lastEntity != null && lastEntity.IsFull)
                {
                    currRowKey++;
                    Logger.Debug("BEGIN: Retrieve cloud table entity async id: '{0}', thread: '{1}', row: '{2}'", streamName, Thread.CurrentThread.ManagedThreadId, currRowKey);
                    entity = await cloudTable.RetrieveAsync(streamName, currRowKey.ToString());
                    Logger.Debug("END: Retrieve cloud table entity async id: '{0}', thread: '{1}', row: '{2}", streamName, Thread.CurrentThread.ManagedThreadId, currRowKey);
                    if (entity != null)
                    {
                        lastEntity = AppendOnlyStoreDynamicTableEntity.Parse(entity);
                        entities.Add(lastEntity);
                    }
                }
            }

            return entities;
        }
    }
}
