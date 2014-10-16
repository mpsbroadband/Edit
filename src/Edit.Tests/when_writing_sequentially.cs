using System;
using System.Collections.Generic;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_sequentially
    {
        private Establish context = () =>
            {
                eventStore = Bootstrapper.WireupEventStore();

                streamName = Guid.NewGuid().ToString();
                eventStore.WriteAsync(streamName, new List<Chunk>(), null).Wait();


                // two readers which will be out of sync
                var chunkset1 = eventStore.ReadAsync(streamName).Result;

                // new write operation
                eventStore.WriteAsync(streamName, chunkset1.Items, chunkset1.Version).Wait();
            };

        private Because of = () =>
            {
                var chunkset2 = eventStore.ReadAsync(streamName).Result;
                eventStore.WriteAsync(streamName, chunkset2.Items, chunkset2.Version).Wait();
                worked = true;
            };

        private It should_have_worked = () =>
            {
                worked.ShouldBeTrue();
            };

        protected static IStreamStore eventStore;
        protected static string streamName;
        protected static bool worked;
    }
}
