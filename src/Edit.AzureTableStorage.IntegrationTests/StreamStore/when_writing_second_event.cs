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
            _causationIdOne = Guid.NewGuid().ToString();
            _causationIdTwo = Guid.NewGuid().ToString();

            AssemblyContext.StreamStore.WriteAsync(_streamName, _causationIdOne, new[] { _eventOne }, null, new CancellationToken()).Await();
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, _causationIdOne, null, new CancellationToken()).Await().AsTask.Result;
        };

        private Because of = () =>
        {
            _version = AssemblyContext.StreamStore.WriteAsync(_streamName, _causationIdTwo, new[] { _eventTwo }, _segment.Version, new CancellationToken()).Await().AsTask.Result;
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, _causationIdOne, null, new CancellationToken()).Await().AsTask.Result;
        };

        private It should_have_both_events_in_the_segment = () => _segment.StreamItems.ShouldContain(_eventOne, _eventTwo);

        private It should_have_the_event_entity_in_the_version = () =>
        {
            _segment.Version.ShouldBeOfExactType<TableStorageVersion>();
            ((TableStorageVersion)_segment.Version).Entities.ShouldNotBeEmpty();
        };

        private It should_return_the_written_version = () => _version.ShouldEqual(_segment.Version);

        private static string _streamName;
        private static StreamSegment<IEvent> _segment;
        private static EventOne _eventOne;
        private static EventTwo _eventTwo;
        private static IVersion _version;
        private static string _causationIdOne;
        private static string _causationIdTwo;
    }
}