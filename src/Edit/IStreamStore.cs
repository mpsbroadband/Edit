using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IStreamStore
    {
        Task WriteAsync<T>(string streamName, IEnumerable<T> items, IVersion expectedVersion, CancellationToken token) where T : class;
        Task<StreamSegment<T>> ReadAsync<T, TSnapshot>(string streamName, SnapshotEnvelope<TSnapshot> snapshot, CancellationToken token) where T : class;
    }
}