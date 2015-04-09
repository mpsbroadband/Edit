using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Version
{
    public class when_creating_a_version_from_zero_entities
    {
        private Establish context = () =>
                                        {
                                            _partitionKey = "agg1";
                                            _entities = new List<DynamicTableEntity>();
                                        };

        private Because of = () =>
                                 {
                                     _version = new TableStorageVersion(_partitionKey, _entities);
                                 };

        private It should_set_partition_key_to_partition_key = () => _version.PartitionKey.ShouldEqual(_partitionKey);

        private It should_set_row_key_to_stream_zero = () => _version.RowKey.ShouldEqual("stream-0");

        private It should_set_column_to_d0 = () => _version.Column.ShouldEqual("d0");

        private It should_set_position_to_zero = () => _version.Position.ShouldEqual(0);

        private static TableStorageVersion _version;
        private static IEnumerable<DynamicTableEntity> _entities;
        private static string _partitionKey;
    }
}