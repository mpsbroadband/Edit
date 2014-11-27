using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface ISnapshotStore
    {
        Task<SnapshotEnvelope<T>> ReadAsync<T>(string id, CancellationToken token);
        Task WriteAsync<T>(string id, SnapshotEnvelope<T> envelope, CancellationToken token);
    }
}