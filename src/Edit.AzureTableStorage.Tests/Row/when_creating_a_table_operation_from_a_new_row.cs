using System.Globalization;
using System.Reflection;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_creating_a_table_operation_from_a_new_row
    {
        private Establish context = () =>
        {
            _streamName = "agg1";
            _sequence = 1;
            _sequencePrefix = "test";
            _row = new BatchOperationRow(_streamName, _sequencePrefix, _sequence, false);
        };

        private Because of = () =>
                                 {
                                     _tableOperation = _row.ToTableOperation();
                                     _entity =
                                         _tableOperation.GetType()
                                                        .GetProperty("Entity",
                                                                     BindingFlags.NonPublic | BindingFlags.Instance)
                                                        .GetValue(_tableOperation) as DynamicTableEntity;
                                 };

        private It should_be_of_type_insert = () => _tableOperation.GetType().GetProperty("OperationType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_tableOperation).ShouldEqual(TableOperationType.Insert);

        private It should_not_have_an_etag = () => _entity.ETag.ShouldBeNull();

        private It should_have_one_property = () => _entity.Properties.Count.ShouldEqual(1);

        private It should_have_stream_name_as_partition_key = () => _entity.PartitionKey.ShouldEqual(_streamName);

        private It should_have_sequence_prefix_and_number_as_row_key = () => _entity.RowKey.ShouldEqual(string.Concat(_sequencePrefix, "-", _sequence.ToString(CultureInfo.InvariantCulture)));

        private static BatchOperationRow _row;
        private static string _streamName;
        private static long _sequence;
        private static TableOperation _tableOperation;
        private static DynamicTableEntity _entity;
        private static string _sequencePrefix;
    }
}