using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.IntegrationTests.SnapshotStore
{
    public class when_writing_and_reading_back_a_snapshot
    {
        private Establish context = () =>
                                        {
                                            _streamName = Guid.NewGuid().ToString();
                                            _rowKey = "1";
                                            _etag = "etag";
                                            _column = "d0";
                                            _data = new byte[100];

                                            _state = new StateOne();
                                            _eventOne = new EventOne("value1");
                                            _eventTwo = new EventTwo("value2");
                                            _state.Apply(_eventOne);
                                            _state.Apply(_eventTwo);
                                            _version = new TableStorageVersion(new List<DynamicTableEntity>
                                                            {
                                                                new DynamicTableEntity(_streamName, _rowKey, _etag,
                                                                                       new Dictionary
                                                                                           <string, EntityProperty>
                                                                                           {
                                                                                               {
                                                                                                   _column,
                                                                                                   new EntityProperty(
                                                                                                   _data)
                                                                                               }
                                                                                           })
                                                            });
                                        };

        private Because of = () =>
                                 {
                                     AssemblyContext.SnapshotStore.WriteAsync(_streamName, _state, _version, new CancellationToken()).Await();
                                     _envelope = AssemblyContext.SnapshotStore.ReadAsync<StateOne>(_streamName, new CancellationToken()).Await().AsTask.Result as TableStorageSnapshotEnvelope<StateOne>;
                                 };

        private It should_have_snapshot_state_in_envelope = () =>
        {
            _envelope.Snapshot.ValueOne.ShouldEqual(_state.ValueOne);
            _envelope.Snapshot.ValueTwo.ShouldEqual(_state.ValueTwo);
        };

        private It should_have_partition_key_in_envelope = () => _envelope.PartitionKey.ShouldEqual(_version.PartitionKey);

        private It should_have_row_key_in_envelope = () => _envelope.RowKey.ShouldEqual(_version.RowKey);

        private It should_have_column_in_envelope = () => _envelope.Column.ShouldEqual(_version.Column);

        private It should_have_position_in_envelope = () => _envelope.Position.ShouldEqual(_version.Position);

        private static string _streamName;
        private static EventOne _event;
        private static StateOne _state;
        private static EventOne _eventOne;
        private static EventTwo _eventTwo;
        private static TableStorageVersion _version;
        private static TableStorageSnapshotEnvelope<StateOne> _envelope;
        private static string _rowKey;
        private static string _etag;
        private static string _column;
        private static byte[] _data;
    }
}