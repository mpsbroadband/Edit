using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Edit
{
    public sealed class StreamStore : IStreamStore
    {
        private readonly IAppendOnlyStore _appendOnlyStore;
        private readonly Framer _framer;
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger(); 

        public StreamStore(IAppendOnlyStore appendOnlyStore, ISerializer serializer)
        {
            _appendOnlyStore = appendOnlyStore;
            _framer = new Framer(serializer);
        }

        #region WriteAsync

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> events, string expectedVersion = null)
        {
            await WriteAsync(streamName, events, Timeout.InfiniteTimeSpan, expectedVersion);
        }

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> events, TimeSpan timeout, string expectedVersion = null)
        {
            await WriteAsync(streamName, events, timeout, CancellationToken.None, expectedVersion);
        }

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> events, CancellationToken token, string expectedVersion = null)
        {
            await WriteAsync(streamName, events, Timeout.InfiniteTimeSpan, token, expectedVersion);
        }

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> events, TimeSpan timeout, CancellationToken token, string expectedVersion = null)
        {
            byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                foreach (var e in events)
                {
                    var result = _framer.Write(e);
                    memoryStream.Write(result, 0, result.Length);
                }

                data = memoryStream.ToArray();
            }

            await _appendOnlyStore.WriteAsync(streamName, data, timeout, token, expectedVersion);
        }

        #endregion


        #region ReadAsync

        public async Task<ChunkSet> ReadAsync(string streamName)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout)
        {
            return await ReadAsync(streamName, timeout, CancellationToken.None);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, CancellationToken token)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan, token);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            Logger.DebugFormat("BEGIN: Read async from the append only store. Streamname : '{0}'", streamName);
            var record = await _appendOnlyStore.ReadAsync(streamName, timeout, token);
            Logger.DebugFormat("END: Read async from the append only store. Streamname : '{0}'", streamName);
            
            if (record == null)
            {
                Logger.DebugFormat("Got a null response. Returning null.");
                return null;
            }

            using (var memoryStream = new MemoryStream(record.Data))
            {
                var chunks = _framer.Read<Chunk>(memoryStream);
                return new ChunkSet(chunks, record.StreamVersion);
            }
        }

        #endregion
    }
}