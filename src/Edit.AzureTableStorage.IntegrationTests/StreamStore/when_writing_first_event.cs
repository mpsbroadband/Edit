using Machine.Specifications;
using System;
using System.Threading;

namespace Edit.AzureTableStorage.IntegrationTests.StreamStore
{
    public class when_writing_first_event
    {
        private Establish context = () =>
        {
            _streamName = Guid.NewGuid().ToString();
            _event = new EventOne("value");
        };

        private Because of = () =>
        {
            AssemblyContext.StreamStore.WriteAsync(_streamName, new[] {_event}, null, new CancellationToken()).Await();
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, null, new CancellationToken()).Await().AsTask.Result;
        };

        private It should_have_the_event_in_the_segment = () => _segment.Items.ShouldContain(_event);

        private It should_have_the_event_entity_in_the_version = () =>
        {
            _segment.Version.ShouldBeOfExactType<TableStorageVersion>();
            ((TableStorageVersion)_segment.Version).Entities.ShouldNotBeEmpty();
        };

        private static string _streamName;
        private static EventOne _event;
        private static StreamSegment<IEvent> _segment;
    }
}