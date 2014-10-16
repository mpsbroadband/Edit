using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage;

namespace Edit.Tests
{
    public class when_doing_concurrent_writes
    {
        private Establish context = () =>
            {
                eventStore = Bootstrapper.WireupEventStore();

                streamName = Guid.NewGuid().ToString();
                eventStore.WriteAsync(streamName, new List<Chunk>(), null).Wait();


                // two readers which will be out of sync
                var chunkset1 = eventStore.ReadAsync(streamName).Result;
                chunkset2 = eventStore.ReadAsync(streamName).Result;

                // new write operation
                eventStore.WriteAsync(streamName, chunkset1.Items, chunkset1.Version).Wait();
            };

        private Because of = () =>
            {
                exception = Catch.Exception(() =>
                    eventStore.WriteAsync(streamName, chunkset2.Items, chunkset2.Version).Wait());
            };

        private It should_have_an_exception = () =>
            {
                exception.ShouldNotBeNull();
            };

        private It should_have_an_precondition_failed_exception = () =>
            {
                var aggregateException = exception as AggregateException;
                var innerException = aggregateException.InnerExceptions.FirstOrDefault() as ConcurrencyException;
                innerException.ShouldNotBeNull();
            };

        protected static IStreamStore eventStore;
        protected static Exception exception;
        protected static string streamName;
        protected static StreamSegment<> chunkset2;
    }
}
