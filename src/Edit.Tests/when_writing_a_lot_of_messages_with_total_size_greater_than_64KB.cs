using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_a_lot_of_messages_with_total_size_greater_than_64KB
    {
        protected static List<TestMessage> aLotOfMessages;
        protected static List<TestMessage> readMessages;
        protected const int NoMessages = 1000;
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
                chunks.Add(new Chunk {Instance = message});
            }
            var streamName = Guid.NewGuid().ToString();

            eventStore.WriteAsync(streamName, chunks, null).Wait();

            var chunkset = eventStore.ReadAsync(streamName).Result;

            readMessages = new List<TestMessage>();
            foreach (var chunk in chunkset.Chunks)
            {
                readMessages.Add(chunk.Instance as TestMessage);
            }

        };

        private It all_messages_are_saved = () =>
            {
                readMessages.Count.ShouldEqual(aLotOfMessages.Count);
            };

        private It last_message_written_is_same_as_last_message_read = () =>
        {
            readMessages.Last().Data.ShouldEqual(aLotOfMessages.Last().Data);
        };


        protected static IStreamStore eventStore;
    }
}
