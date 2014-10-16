using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edit.AzureTableStorage;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_a_single_message_larger_than_64KB
    {
        protected static TestMessage hugeMessage;
        protected static TestMessage readMessage;

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            hugeMessage = new TestMessage();
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < DataColumn.MaxSize*3; i++)
            {
                sb.Append((byte) 1);
            }
            hugeMessage.Data = sb.ToString();
        };

        private Because of = () =>
            {
                List<Chunk> chunks = new List<Chunk>() { new Chunk { Instance = hugeMessage } };
                var streamName = Guid.NewGuid().ToString();

                exception = Catch.Exception(() =>
                    eventStore.WriteAsync(streamName, chunks, null).Wait());

                var chunkset = eventStore.ReadAsync(streamName).Result;
                readMessage = chunkset.Items.First().Instance as TestMessage;
            };

        private It should_have_no_exception = () =>
        {
            exception.ShouldBeNull();
        };

        private It the_message_read_should_be_the_same = () =>
            {
                readMessage.Data.ShouldEqual(hugeMessage.Data);
            };

        protected static IStreamStore eventStore;
        protected static Exception exception;
    }
}
