using System;
using System.IO;
using Newtonsoft.Json;

namespace Edit.Protobuf
{
    public class JsonNetSerializer : ISerializer
    {
        public JsonNetSerializer(JsonSerializerSettings settings)
        {
            serializer = JsonSerializer.Create(settings);
        }

        private JsonSerializer serializer;

        public void Serialize<T>(T instance, Stream target) where T : class
        {
            var jsonTextWriter = new JsonTextWriter(new StreamWriter(target));
            serializer.Serialize(jsonTextWriter, instance);
            jsonTextWriter.Flush();
        }

        public T Deserialize<T>(Stream source)
        {
            return serializer.Deserialize<T>(new JsonTextReader(new StreamReader(source)));
        }

        public object Deserialize(Type type, Stream source)
        {
            return serializer.Deserialize(new JsonTextReader(new StreamReader(source)), type);
        }
    }
}