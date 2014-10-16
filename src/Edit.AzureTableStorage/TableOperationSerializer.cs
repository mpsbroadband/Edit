using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.IO;

namespace Edit.AzureTableStorage
{
    public class TableOperationSerializer
    {
        private const byte Delimeter = 0x4;

        private readonly ISerializer _serializer;

        public TableOperationSerializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public TableBatchOperation Serialize<T>(string streamName, IEnumerable<T> items, TableStorageVersion expectedVersion) where T : class
        {
            var writer = new BatchOperationWriter(streamName, expectedVersion.Entities);

            using (var stream = new MemoryStream())
            {
                foreach (var item in items)
                {
                    _serializer.Serialize(item, stream);
                    stream.WriteByte(Delimeter);
                }

                stream.Position = 0;

                writer.Write(stream);
            }

            return writer.ToBatchOperation();
        }
    }
}