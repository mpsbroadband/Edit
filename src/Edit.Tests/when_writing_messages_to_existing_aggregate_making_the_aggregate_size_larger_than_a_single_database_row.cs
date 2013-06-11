using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_messages_to_existing_aggregate_making_the_aggregate_size_larger_than_a_single_database_row
    {
        protected static List<Chunk> chunkMessages = new List<Chunk>();
        protected static TestMessage lastWrittenMessage = new TestMessage { Data = "Last Message" };
        protected static ChunkSet readChunks;
        protected static List<TestMessage> readMessages = new List<TestMessage>();
        protected const int NoMessages = 2831;

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            var messages = TestMessage.CreateTestMessages(NoMessages);
            foreach (var message in messages)
            {
                chunkMessages.Add(new Chunk { Instance = message });
            }
            streamName = Guid.NewGuid().ToString();

            eventStore.WriteAsync(streamName, chunkMessages.Take(NoMessages/2), null).Wait();
            chunkMessages.Add(new Chunk{ Instance = lastWrittenMessage });

            readChunks = eventStore.ReadAsync(streamName).Result;
        };

        private Because of = () =>
        {
            eventStore.WriteAsync(streamName, chunkMessages, readChunks.Version).Wait();

            var chunkset = eventStore.ReadAsync(streamName).Result;

            foreach (var chunk in chunkset.Chunks)
            {
                readMessages.Add(chunk.Instance as TestMessage);
            }
        };

        private It all_messages_are_saved = () =>
        {
            readMessages.Count.ShouldEqual(chunkMessages.Count);
        };

        private It last_message_written_is_same_as_last_message_read = () =>
        {
            readMessages.Last().Data.ShouldEqual(lastWrittenMessage.Data);
        };


        protected static IStreamStore eventStore;
        protected static String streamName;
    }
}
