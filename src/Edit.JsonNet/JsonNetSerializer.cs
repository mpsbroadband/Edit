using System;
using System.IO;
using Newtonsoft.Json;

namespace Edit.JsonNet
{
    public class JsonNetSerializer : ISerializer
    {
        public JsonNetSerializer(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }
        private readonly JsonSerializer _serializer;

        public void Serialize<T>(T instance, Stream target) where T : class
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms) { AutoFlush = true })
            {
                _serializer.Serialize(sw, instance);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(target);
            }
        }

        public T Deserialize<T>(Stream source)
        {
            using (var sr = new StreamReader(source))
                return _serializer.Deserialize<T>(new JsonTextReader(sr));
        }

        public object Deserialize(Type type, Stream source)
        {
            using (var sr = new StreamReader(source))
                return _serializer.Deserialize(new JsonTextReader(sr), type);
        }
    }
}