using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface ISnapshotStore
    {
        Task<ISnapshotEnvelope<T>> ReadAsync<T>(string id, CancellationToken token);
        Task WriteAsync<T>(string id, T snapshot, IVersion version, CancellationToken token);
    }
}