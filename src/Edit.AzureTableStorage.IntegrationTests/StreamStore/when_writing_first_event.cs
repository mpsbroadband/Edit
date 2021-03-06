﻿using Machine.Specifications;
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
            _causationId = Guid.NewGuid().ToString();
        };

        private Because of = () =>
        {
            _version = AssemblyContext.StreamStore.WriteAsync(_streamName, _causationId, new[] {_event}, null, new CancellationToken()).Await().AsTask.Result;
            _segment = AssemblyContext.StreamStore.ReadAsync<IEvent, IState>(_streamName, _causationId, null, new CancellationToken()).Await().AsTask.Result;
        };

        private It should_have_the_event_in_the_segment = () => _segment.StreamItems.ShouldContain(_event);

        private It should_have_the_event_entity_in_the_version = () =>
        {
            _segment.Version.ShouldBeOfExactType<TableStorageVersion>();
            ((TableStorageVersion)_segment.Version).Entities.ShouldNotBeEmpty();
        };

        private It should_return_the_written_version = () => _version.ShouldEqual(_segment.Version);

        private static string _streamName;
        private static EventOne _event;
        private static StreamSegment<IEvent> _segment;
        private static IVersion _version;
        private static string _causationId;
    }
}