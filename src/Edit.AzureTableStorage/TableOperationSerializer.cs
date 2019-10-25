using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.IO;
using AzureApi.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class TableOperationSerializer : ITableOperationSerializer
    {
        private const byte Delimeter = 0x4;

        private readonly ISerializer _serializer;

        public TableOperationSerializer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public TableBatchOperation Serialize<T>(string streamName, string sequencePrefix, IEnumerable<T> items, IEnumerable<DynamicTableEntity> existingEntities, bool developmentStorage) where T : class
        {
            var writer = new BatchOperationWriter(streamName, sequencePrefix, existingEntities, developmentStorage);

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

        public IEnumerable<T> Deserialize<T>(IEnumerable<DynamicTableEntity> entities, string column, int position)
        {
            using (var stream = new MemoryStream())
            {
                foreach (var property in entities.SelectMany(e => e.Properties.OrderByAlphaNumeric(p => p.Key)))
                {
                    var data = property.Value.BinaryValue;

                    if (stream.Length == 0 && (property.Key == column || column == null))
                    {
                        stream.Write(data, position, data.Length - position);
                    }
                    else if (stream.Length > 0)
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                stream.Position = 0;

                int read;
                var buffer = new byte[1024];
                var streams = new List<Stream>
                                  {
                                      new MemoryStream()
                                  };

                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (var i = 0; i < read; i++)
                    {
                        var b = buffer[i];
                        if (b != Delimeter)
                        {
                            streams.Last().WriteByte(b);
                        }
                        else
                        {
                            streams.Add(new MemoryStream());
                        }
                    }
                }

                return streams.Where(s => s.Length > 0)
                    .Select(s =>
                    {
                        using (s)
                        {
                            s.Position = 0;
                            return _serializer.Deserialize<object>(s);
                        }
                    })
                    .OfType<T>()
                    .ToList();
            }
        }
    }
}