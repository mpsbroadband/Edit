using System;
using System.Collections.Generic;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_multiple_messages
    {
        protected static List<TestMessage> writeMessages;
        protected static List<TestMessage> readMessages;

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            writeMessages = new List<TestMessage>
            {
                new TestMessage() { Data = "First" }, 
                new TestMessage() { Data = "Second" },
                new TestMessage() { Data = "Third" }
            };
            List<Chunk> chunks = new List<Chunk>();
            foreach (var writeMessage in writeMessages)
            {
                chunks.Add(new Chunk { Instance = writeMessage });
            }

            streamName = Guid.NewGuid().ToString();
            eventStore.WriteAsync(streamName, chunks, null).Wait();
        };

        private Because of = () =>
        {
            readMessages = new List<TestMessage>();
            var chunkset = eventStore.ReadAsync(streamName).Result;

            foreach (var chunk in chunkset.Items)
            {
                readMessages.Add(chunk.Instance as TestMessage);
            }
        };

        private It all_messages_should_be_saved = () =>
            {
                readMessages.Count.ShouldEqual(writeMessages.Count);
            };

        private It all_messages_should_be_stored_in_correct_order = () =>
        {
            for (int i = 0; i < writeMessages.Count; i++)
            {
                writeMessages[i].Data.ShouldEqual(readMessages[i].Data);
            }
        };

        protected static IStreamStore eventStore;
        protected static String streamName;
    }
}
