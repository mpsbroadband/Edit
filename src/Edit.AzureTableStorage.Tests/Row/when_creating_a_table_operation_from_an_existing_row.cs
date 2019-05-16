using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_creating_a_table_operation_from_an_existing_row
    {
        private Establish context = () =>
        {
            _entity = new DynamicTableEntity("agg1", "test-1", "etag",
                                             new Dictionary<string, EntityProperty>
                                                 {
                                                     {
                                                         "d1",
                                                         new EntityProperty(new byte[0])
                                                     }
                                                 });
            _row = new BatchOperationRow(_entity, false);
        };

        private Because of = () =>
                                 {
                                     _tableOperation = _row.ToTableOperation();
                                     _tableOperationEntity =
                                         _tableOperation.GetType()
                                                        .GetProperty("Entity",
                                                                     BindingFlags.NonPublic | BindingFlags.Instance)
                                                        .GetValue(_tableOperation) as DynamicTableEntity;
                                 };

        private It should_be_of_type_replace = () => _tableOperation.GetType().GetProperty("OperationType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_tableOperation).ShouldEqual(TableOperationType.Replace);

        private It should_have_existing_etag = () => _tableOperationEntity.ETag.ShouldEqual(_entity.ETag);

        private It should_have_data_properties = () => _tableOperationEntity.Properties.Where(p => p.Key.StartsWith("d")).ShouldNotBeEmpty();

        private It should_have_existing_entity_partition_key = () => _tableOperationEntity.PartitionKey.ShouldEqual(_entity.PartitionKey);

        private It should_have_existing_entity_row_key = () => _tableOperationEntity.RowKey.ShouldEqual(_entity.RowKey);

        private static BatchOperationRow _row;
        private static TableOperation _tableOperation;
        private static DynamicTableEntity _entity;
        private static DynamicTableEntity _tableOperationEntity;
    }
}