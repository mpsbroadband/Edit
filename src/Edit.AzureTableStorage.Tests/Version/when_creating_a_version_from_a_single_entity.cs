using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Version
{
    public class when_creating_a_version_from_a_single_entity
    {
        private Establish context = () =>
                                        {
                                            _partitionKey = "agg1";
                                            _rowKey = "1";
                                            _etag = "etag";
                                            _column = "d1";
                                            _data = new byte[100];
                                            _entities = new List<DynamicTableEntity>
                                                            {
                                                                new DynamicTableEntity(_partitionKey, _rowKey, _etag,
                                                                                       new Dictionary
                                                                                           <string, EntityProperty>
                                                                                           {
                                                                                               {
                                                                                                   _column,
                                                                                                   new EntityProperty(
                                                                                                   _data)
                                                                                               }
                                                                                           })
                                                            };
                                        };

        private Because of = () =>
                                 {
                                     _version = new TableStorageVersion(_entities);
                                 };

        private It should_set_partition_key_to_entity_partition_key = () => _version.PartitionKey.ShouldEqual(_partitionKey);

        private It should_set_row_key_to_entity_row_key = () => _version.RowKey.ShouldEqual(_rowKey);

        private It should_set_column_to_last_entity_property_key = () => _version.Column.ShouldEqual(_column);

        private It should_set_position_to_last_entity_property_data_length = () => _version.Position.ShouldEqual(_data.Length);

        private static TableStorageVersion _version;
        private static IEnumerable<DynamicTableEntity> _entities;
        private static string _partitionKey;
        private static string _rowKey;
        private static string _etag;
        private static string _column;
        private static byte[] _data;
    }
}