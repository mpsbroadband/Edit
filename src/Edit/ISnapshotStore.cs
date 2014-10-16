using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface ISnapshotStore
    {
        Task<T> ReadAsync<T>(string id, TimeSpan timeout, CancellationToken token);
        Task<T> WriteAsync<T>(string id, T snapshot, TimeSpan timeout, CancellationToken token);
    }
}