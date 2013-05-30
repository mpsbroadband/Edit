using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IAppendOnlyStore : IDisposable
    {
        Task WriteAsync(string streamName, byte[] data, IStoredDataVersion expectedVersion);
        Task WriteAsync(string streamName, byte[] data, TimeSpan timeout, IStoredDataVersion expectedVersion);
        Task WriteAsync(string streamName, byte[] data, CancellationToken token, IStoredDataVersion expectedVersion);
        Task WriteAsync(string streamName, byte[] data, TimeSpan timeout, CancellationToken token, IStoredDataVersion expectedVersion);

        Task<Record> ReadAsync(string streamName);
        Task<Record> ReadAsync(string streamName, TimeSpan timeout);
        Task<Record> ReadAsync(string streamName, CancellationToken token);
        Task<Record> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token);
    }
}
