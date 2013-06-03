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

        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            hugeMessage = new TestMessage();
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < DataColumn.MaxSize+1; i++)
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

            };

        private It should_have_an_exception = () =>
        {
            exception.ShouldNotBeNull();
        };

        private It should_have_a_storage_size_exception = () =>
        {
            var aggregateException = exception as AggregateException;
            var innerException = aggregateException.InnerExceptions.FirstOrDefault() as StorageSizeException;
            innerException.ShouldNotBeNull();
        };

        protected static IStreamStore eventStore;
        protected static Exception exception;
    }
}
