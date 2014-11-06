using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_writing_too_much_to_an_existing_row
    {
        private Establish context = () =>
        {
            _existingData = new byte[new BatchOperationRow("a", 0).MaxSize - 1];
            _dataToWrite = new byte[] { 1, 2, 3, 4};
            _overflowData = new byte[] { 2, 3, 4 };

            new Random().NextBytes(_existingData);

            _entity = new DynamicTableEntity("agg1", "1", "etag",
                                 new Dictionary<string, EntityProperty>
                                                 {
                                                     {
                                                         "d1",
                                                         new EntityProperty(_existingData)
                                                     }
                                                 });
            _row = new BatchOperationRow(_entity);
        };

        private Because of = () => _dataLeft = _row.Write(_dataToWrite, 0);

        private It should_have_columns = () => _row.Columns.ShouldNotBeEmpty();

        private It should_have_size_of_max_data = () => _row.Size.ShouldEqual(_existingData.Length + 1);

        private It should_be_dirty = () => _row.IsDirty.ShouldBeTrue();

        private It should_return_overflow_data = () => _dataLeft.ShouldEqual(_overflowData);

        private static BatchOperationRow _row;
        private static byte[] _existingData;
        private static byte[] _dataLeft;
        private static byte[] _overflowData;
        private static DynamicTableEntity _entity;
        private static byte[] _dataToWrite;
    }
}