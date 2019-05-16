using Machine.Specifications;
using System;
using System.Threading;

namespace Edit.AzureTableStorage.IntegrationTests.StreamStore
{
    public class when_writing_zero_events
    {
        private Establish context = () =>
        {
            _streamName = Guid.NewGuid().ToString();
            _causationId = Guid.NewGuid().ToString();
        };

        private Because of = () =>
        {
            _version = AssemblyContext.StreamStore.WriteAsync(_streamName, _causationId, new IEvent[0], null, new CancellationToken()).Await().AsTask.Result;
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, _causationId, null, new CancellationToken()).Await().AsTask.Result;
        };

        private It should_return_the_written_version = () => _version.ShouldEqual(_segment.Version);

        private static string _streamName;
        private static StreamSegment<IEvent> _segment;
        private static IVersion _version;
        private static string _causationId;
    }
}