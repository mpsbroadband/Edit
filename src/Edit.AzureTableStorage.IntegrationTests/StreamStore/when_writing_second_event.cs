using Machine.Specifications;
using System;
using System.Threading;

namespace Edit.AzureTableStorage.IntegrationTests.StreamStore
{
    public class when_writing_second_event
    {
        private Establish context = () =>
        {
            _streamName = Guid.NewGuid().ToString();
            _eventOne = new EventOne("value1");
            _eventTwo = new EventTwo("value2");

            AssemblyContext.StreamStore.WriteAsync(_streamName, new[] { _eventOne }, null, new CancellationToken()).Await();
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, null, new CancellationToken()).Await().AsTask.Result;
        };

        private Because of = () =>
        {
            AssemblyContext.StreamStore.WriteAsync(_streamName, new[] { _eventTwo }, _segment.Version, new CancellationToken()).Await();
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, null, new CancellationToken()).Await().AsTask.Result;
        };

        private It should_have_both_events_in_the_segment = () => _segment.Items.ShouldContain(_eventOne, _eventTwo);

        private It should_have_the_event_entity_in_the_version = () =>
        {
            _segment.Version.ShouldBeOfExactType<TableStorageVersion>();
            ((TableStorageVersion)_segment.Version).Entities.ShouldNotBeEmpty();
        };

        private static string _streamName;
        private static StreamSegment<IEvent> _segment;
        private static EventOne _eventOne;
        private static EventTwo _eventTwo;
    }
}