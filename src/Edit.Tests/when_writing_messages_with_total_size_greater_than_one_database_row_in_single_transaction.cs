using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_messages_with_total_size_greater_than_one_database_row_in_single_transaction
    {
        protected static List<TestMessage> aLotOfMessages = new List<TestMessage>();
        protected const int NoMessages = 10000;
        private Establish context = () =>
        {
            eventStore = Bootstrapper.WireupEventStore();
            aLotOfMessages = TestMessage.CreateTestMessages(NoMessages);
        };

        private Because of = () =>
        {
            List<Chunk> chunks = new List<Chunk>();
            foreach (var message in aLotOfMessages)
            {
                chunks.Add(new Chunk { Instance = message });
            }
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


        protected static Exception exception;
        protected static IStreamStore eventStore;
    }
}
