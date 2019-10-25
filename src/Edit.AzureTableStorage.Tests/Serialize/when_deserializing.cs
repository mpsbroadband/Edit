using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Edit.JsonNet;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Edit.AzureTableStorage.Tests.Serialize
{
    class when_deserializing
    {
        private static TableOperationSerializer _serializer;
        private static IEnumerable<IA> _result;
        private static IEnumerable<DynamicTableEntity> _entities;
        private static List<Guid> _ids;

        private Establish context = () =>
        {
            var serializer = new JsonNetSerializer(
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

            _ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            byte[] entity1;
            byte[] entity2;
            byte[] entity3;
            using (var stream = new MemoryStream())
            {
                entity1 = ToBytes(serializer, stream, new Type1
                {
                    Id = _ids.First()
                });

                stream.WriteByte(0x4);

                entity2 = ToBytes(serializer, stream, new Type1
                {
                    Id = _ids.Last()
                });

                stream.WriteByte(0x4);

                entity3 = ToBytes(serializer, stream, new Type2
                {
                    Id = Guid.NewGuid()
                });
            }

            _entities = new[]
            {
                new DynamicTableEntity
                {
                    Properties = new Dictionary<string, EntityProperty>
                    {
                        {"partitionKey1", new EntityProperty(entity1)},
                        {"partitionKey2", new EntityProperty(entity2)},
                        {"partitionKey3", new EntityProperty(entity3)}
                    }
                }
            };

            _serializer = new TableOperationSerializer(serializer);
        };

        private Because of = () => _result = _serializer.Deserialize<IA>(_entities, "partitionKey1", 0);

        private It the_result_contains_the_valid_items =
            () => _result.ToList().Select(e => e.Id).Distinct().ShouldContainOnly(_ids);

        private static byte[] ToBytes(ISerializer serializer, Stream stream, object entity)
        {
            serializer.Serialize(entity, stream);

            stream.Position = 0;

            var bytes = new byte[stream.Length];

            while (stream.Read(bytes, 0, (int) stream.Length) > 0)
            {
            }

            return bytes;
        }
    }
}