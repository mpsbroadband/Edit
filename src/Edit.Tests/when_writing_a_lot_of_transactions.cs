using System;
using System.Collections.Generic;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_a_lot_of_transactions
    {
        protected static List<TestMessage> writeMessages = new List<TestMessage>();
        private static readonly byte[] PayLoad = new byte[1024 * 50]; // Each message has a 50 KB payload
        private const int NoTransactions = 20;

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            writeMessages = TestMessage.CreateTestMessages(NoTransactions);
            streamName = Guid.NewGuid().ToString();
            var chunks = new List<Chunk>();
            ChunkSet readChunks = null;
            foreach (var message in writeMessages)
            {
                message.PayLoad = PayLoad;
                chunks.Add(new Chunk { Instance = message });
                var version = readChunks == null ? null : readChunks.Version;
                eventStore.WriteAsync(streamName, chunks, version).Wait();
                readChunks = eventStore.ReadAsync(streamName).Result;
            }
        };

        private Because of = () =>
        {
            readMessages = eventStore.ReadAsync(streamName).Result;
        };

        private It the_read_messages_should_match_the_written_ones = () =>
        {
            int i = 0;
            foreach (Chunk chunk in readMessages.Chunks)
            {
                (chunk.Instance as TestMessage).Data.ShouldEqual(writeMessages[i].Data);
                i++;
            }
            i.ShouldEqual(writeMessages.Count);
        };

        protected static ChunkSet readMessages;
        protected static String streamName;
        protected static IStreamStore eventStore;
    }
}
