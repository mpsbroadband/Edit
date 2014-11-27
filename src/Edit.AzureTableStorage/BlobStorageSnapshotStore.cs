using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Edit.AzureTableStorage
{
    public class BlobStorageSnapshotStore : ISnapshotStore
    {
        private readonly ISerializer _serializer;
        private readonly CloudBlobClient _client;
        private readonly CloudBlobContainer _container;

        public BlobStorageSnapshotStore(ISerializer serializer, CloudStorageAccount storageAccount)
        {
            _serializer = serializer;
            _client = storageAccount.CreateCloudBlobClient();
            _container = _client.GetContainerReference("SnapshotStore");
            _container.CreateIfNotExists();
        }

        public async Task<SnapshotEnvelope<T>> ReadAsync<T>(string id, CancellationToken token)
        {
            var reference = await _container.GetBlobReferenceFromServerAsync(id);
            if (await reference.ExistsAsync())
            {
                using (var stream = await reference.OpenReadAsync(token))
                {
                    return _serializer.Deserialize<SnapshotEnvelope<T>>(stream);
                }
            }
            else
            {
                return null;
            }
        }

        public async Task WriteAsync<T>(string id, SnapshotEnvelope<T> envelope, CancellationToken token)
        {
            using (var stream = await _container.GetBlockBlobReference(id)
                                                .OpenWriteAsync(token))
            {
                _serializer.Serialize(envelope, stream);
            }
        }
    }
}