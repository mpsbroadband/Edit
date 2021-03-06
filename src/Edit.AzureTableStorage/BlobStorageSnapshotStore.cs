﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Edit.AzureTableStorage
{
    public class BlobStorageSnapshotStore : ISnapshotStore
    {
        private readonly ISerializer _serializer;
        private readonly CloudBlobContainer _container;

        public BlobStorageSnapshotStore(ISerializer serializer, CloudStorageAccount storageAccount, string containerName = "snapshot-store")
        {
            _serializer = serializer;
            var client = storageAccount.CreateCloudBlobClient();
            _container = client.GetContainerReference(containerName);
            _container.CreateIfNotExists();
        }

        public async Task<ISnapshotEnvelope<T>> ReadAsync<T>(string id, CancellationToken token)
        {
            var reference = _container.GetBlockBlobReference(id);

            if (await reference.ExistsAsync(token))
            {
                using (var stream = await reference.OpenReadAsync(token))
                {
                    return _serializer.Deserialize<ISnapshotEnvelope<T>>(stream);
                }
            }

            return null;
        }

        public async Task WriteAsync<T>(string id, T snapshot, IVersion version, CancellationToken token)
        {
            var tableVersion = version as TableStorageVersion;

            if (tableVersion == null)
            {
                throw new ArgumentException("Only Azure Table Storage versions are supported.", "version");
            }

            using (var stream = await _container.GetBlockBlobReference(id)
                                                .OpenWriteAsync(token))
            {
                _serializer.Serialize(
                    new TableStorageSnapshotEnvelope<T>(tableVersion.PartitionKey, tableVersion.RowKey, tableVersion.Column,
                                            tableVersion.Position, snapshot), stream);
            }
        }
    }
}