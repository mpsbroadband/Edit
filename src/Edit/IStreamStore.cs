using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IStreamStore
    {
        Task WriteAsync<T>(string streamName, IEnumerable<T> items, IVersion expectedVersion, TimeSpan timeout, CancellationToken token);
        Task<StreamSegment<T>> ReadAsync<T>(string streamName, TimeSpan timeout, CancellationToken token);
    }
}