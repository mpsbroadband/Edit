using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edit.AzureTableStorage;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_a_combination_of_small_and_large_messages
    {
        protected static List<TestMessage> writeMessages = new List<TestMessage>();

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < DataColumn.MaxSize + 1; i++)
            {
                sb.Append((byte)1);
            }
            writeMessages = new List<TestMessage>
                {
                    new TestMessage {Data = "small 1"},
                    new TestMessage {Data = "small 2"},
                    new TestMessage {Data = "large 1" + sb.ToString()},
                    new TestMessage {Data = "small 3"},
                    new TestMessage {Data = "large 2" + sb.ToString() + "2"},
                    new TestMessage {Data = "small 4"},
                    new TestMessage {Data = "small 5"},
                };
        };

        private Because of = () =>
            {
                var chunks = writeMessages.Select(message => new Chunk {Instance = message}).ToList();
                var streamName = Guid.NewGuid().ToString();

            exception = Catch.Exception(() =>
                eventStore.WriteAsync(streamName, chunks, null).Wait());

            _chunkset = eventStore.ReadAsync(streamName).Result;
        };

        private It should_have_no_exception = () =>
        {
            exception.ShouldBeNull();
        };

        private It the_messages_read_should_be_the_same = () =>
            {
                int cnt = 0;
                foreach (var chunk in _chunkset.Items)
                {
                    writeMessages[cnt].Data.ShouldEqual((chunk.Instance as TestMessage).Data);
                    cnt++;
                }
            };

        protected static IStreamStore eventStore;
        protected static Exception exception;
        protected static StreamSegment<> _chunkset;
    }
}
