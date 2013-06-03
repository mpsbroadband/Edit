using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    public class AzureTableStorageFramer : IFramer
    {
        private readonly ISerializer _serializer;
        private readonly SHA1Managed _sha1Managed = new SHA1Managed();

        public AzureTableStorageFramer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public byte[] Write<T>(T frame) where T : class
        {
            byte[] eSerialized;

            using (var memoryStream = new MemoryStream())
            {
                _serializer.Serialize(frame, memoryStream);
                eSerialized = memoryStream.ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var binary = new BinaryWriter(memoryStream))
                {
                    binary.Write(eSerialized.Length); // length of data in int
                    binary.Write(eSerialized); // the actual data

                    var data = new byte[memoryStream.Position];
                    memoryStream.Seek(0, SeekOrigin.Begin); //rewind stream
                    memoryStream.ReadAsync(data, 0, data.Length); // read to data

                    var hash = ComputeHash(data);
                    binary.Write(hash); // write hash to stream

                    return memoryStream.ToArray();
                }
            }
        }

        private IEnumerable<T> Read<T>(byte[] dataFrames) where T : class
        {
            using (var source = new MemoryStream(dataFrames))
            {
                var frames = new List<T>();
                var binary = new BinaryReader(source);

                source.Seek(0, SeekOrigin.Begin); // make sure the stream is at position 0

                while (source.Length > source.Position)
                {
                    var length = binary.ReadInt32();
                    var bytes = binary.ReadBytes(length);

                    var data = new byte[source.Position];
                    source.Seek(0, SeekOrigin.Begin);
                    source.ReadAsync(data, 0, data.Length);

                    var actualHash = ComputeHash(data);

                    var hash = binary.ReadBytes(20);

                    if (!hash.SequenceEqual(actualHash))
                    {
                        // This is broken, but it doesn't really matter. 
                        // Shall we log it ?
                    }
                    using (var memoryStream = new MemoryStream(bytes))
                    {
                        var e = _serializer.Deserialize<T>(memoryStream);
                        frames.Add(e);
                    }
                }

                return frames;
            }
        }

        private IEnumerable<T> ReadColumns<T>(IEnumerable<Func<byte[]>> columns) where T : class
        {
            var frames = new List<T>();
            foreach (var column in columns)
            {
                var columnFrames = ReadColumn<T>(column);
                int cnt = 0;
                foreach (var columnFrame in columnFrames)
                {
                    cnt++;
                    frames.Add(columnFrame);
                }
                if (cnt == 0)
                {
                    break;
                }
            }
            return frames;
        }

        private IEnumerable<T> ReadColumn<T>(Func<byte[]> column) where T : class
        {
            byte[] data = column();
            if (data != null)
            {
                return Read<T>(data);
            }
            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> Read<T>(AppendOnlyStoreTableEntity entity) where T : class
        {
            var columns = new ColumnsWrapper(entity);
            return ReadColumns<T>(columns.DataColumns);
        }

        public AppendOnlyStoreTableEntity Write<T>(IEnumerable<T> frames) where T : class
        {
            byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                foreach (var frame in frames)
                {
                    var result = Write(frame);
                    memoryStream.Write(result, 0, result.Length);
                }

                data = memoryStream.ToArray();
            }
            var entity = new AppendOnlyStoreTableEntity
                {
                    Data = data
                };
            return entity;
        }

        private byte[] ComputeHash(byte[] data)
        {
            return _sha1Managed.ComputeHash(data);
        }

    }
}
