using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    public class TableStorageStreamStore : IStreamStore
    {
        public Task WriteAsync<T>(string streamName, IEnumerable<T> items, IVersion expectedVersion, 
                                  TimeSpan timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<StreamSegment<T>> ReadAsync<T>(string streamName, TimeSpan timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}