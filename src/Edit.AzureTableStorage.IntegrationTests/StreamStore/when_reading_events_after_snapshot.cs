using System;
using System.Threading;
using Machine.Specifications;

namespace Edit.AzureTableStorage.IntegrationTests.StreamStore
{
    public class when_reading_events_after_snapshot
    {
        private Establish context = () =>
        {
            _streamName = Guid.NewGuid().ToString();
            _eventOne = new EventOne("value1");
            _eventTwo = new EventTwo("value2");
            _causationIdOne = Guid.NewGuid().ToString();
            _causationIdTwo = Guid.NewGuid().ToString();

            // write first event
            AssemblyContext.StreamStore.WriteAsync(_streamName, _causationIdOne, new[] { _eventOne }, null, new CancellationToken()).Await();
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, StateOne>(_streamName, _causationIdOne, null, new CancellationToken()).Await().AsTask.Result;

            // replay and create snapshot from stream
            var state = new StateOne();

            foreach (var e in _segment.StreamItems)
            {
                ((dynamic)state).Apply((dynamic)e);
            }

            AssemblyContext.SnapshotStore.WriteAsync(_streamName, state, _segment.Version, new CancellationToken()).Await();
            _envelope = AssemblyContext.SnapshotStore.ReadAsync<StateOne>(_streamName, new CancellationToken()).Await().AsTask.Result;

            // write second event
            AssemblyContext.StreamStore.WriteAsync(_streamName, _causationIdTwo, new[] { _eventTwo }, _segment.Version, new CancellationToken()).Await();
        };

        private Because of = () =>
        {
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, StateOne>(_streamName, _causationIdOne, _envelope, new CancellationToken()).Await().AsTask.Result;
        };

        private It should_only_have_event_after_snapshot_in_the_segment = () => _segment.StreamItems.ShouldContainOnly(_eventTwo);

        private static string _streamName;
        private static EventOne _eventOne;
        private static StreamSegment<IEvent> _segment;
        private static EventTwo _eventTwo;
        private static ISnapshotEnvelope<StateOne> _envelope;
        private static string _causationIdOne;
        private static string _causationIdTwo;
    }
}