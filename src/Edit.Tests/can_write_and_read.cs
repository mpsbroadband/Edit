using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage;

namespace Edit.Tests
{
    public class TestMessage
    {
        public string Data { get; set; }

        public static List<TestMessage> CreateTestMessages(int noMessages)
        {
            var messages = new List<TestMessage>();
            for (int i = 0; i < noMessages; i++)
            {
                messages.Add(new TestMessage { Data = "Message number " + i });
            }
            return messages;
        }
    }

    public class can_write_and_read
    {
        protected static TestMessage message = new TestMessage() {Data = "Love it"};
        protected static TestMessage readMessage;

        private Establish context = () =>
            {
                eventStore = Bootstrapper.WireupEventStore();
            };

        private Because of = () =>
            {
                var chunks = new List<Chunk>();
                chunks.Add(new Chunk() { Instance = message});

                var streamName = Guid.NewGuid().ToString();
                eventStore.WriteAsync(streamName, chunks, null).Wait();

                var chunkset1 = eventStore.ReadAsync(streamName).Result;
                readMessage = chunkset1.Chunks.First().Instance as TestMessage;
            };

        private It the_data_should_be_equal = () =>
            {
                readMessage.Data.ShouldEqual(message.Data);
            };

        protected static IStreamStore eventStore;
    }
}
