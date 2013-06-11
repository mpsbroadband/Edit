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
        protected const int NoMessages = 8000; //2831;

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            var messages = TestMessage.CreateTestMessages(NoMessages);
            foreach (var message in messages)
            {
                chunkMessages.Add(new Chunk { Instance = message });
            }
            streamName = Guid.NewGuid().ToString();

            // Write first db row
            eventStore.WriteAsync(streamName, chunkMessages.Take(NoMessages/4), null).Wait();

            readChunks = eventStore.ReadAsync(streamName).Result;

            // Expanded to 2 rows
            eventStore.WriteAsync(streamName, chunkMessages.Take(NoMessages / 2), readChunks.Version).Wait();

            readChunks = eventStore.ReadAsync(streamName).Result;

            // Expanded to 3 rows
            int noMess = (int) ((NoMessages/4)*3);
            eventStore.WriteAsync(streamName, chunkMessages.Take(noMess), readChunks.Version).Wait();

            readChunks = eventStore.ReadAsync(streamName).Result;

            // Expanded to 4 rows
            chunkMessages.Add(new Chunk { Instance = lastWrittenMessage });
            eventStore.WriteAsync(streamName, chunkMessages, readChunks.Version).Wait();
        };

        private Because of = () =>
        {

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

        private It the_messages_read_are_the_same_as_the_ones_written = () =>
        {
            for (int i = 0; i < chunkMessages.Count; i++)
            {
                (chunkMessages[i].Instance as TestMessage).Data.ShouldEqual(readMessages[i].Data);
            }
            //readMessages.Last().Data.ShouldEqual(lastWrittenMessage.Data);
        };


        protected static IStreamStore eventStore;
        protected static String streamName;
    }
}
