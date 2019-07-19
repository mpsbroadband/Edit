using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IStreamStore
    {
        Task<IVersion> WriteAsync<T>(string streamName, string causationId, IEnumerable<T> items, IVersion expectedVersion, CancellationToken token) where T : class;
        Task<StreamSegment<T>> ReadAsync<T, TSnapshot>(string streamName, string causationId, ISnapshotEnvelope<TSnapshot> snapshot, CancellationToken token) where T : class;
    }
}