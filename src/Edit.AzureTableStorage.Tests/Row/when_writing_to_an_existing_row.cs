using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_writing_to_an_existing_row
    {
        private Establish context = () =>
        {
            _dataToWrite = new byte[] { 4, 5, 6 };
            _existingData = new byte[] { 1, 2, 3 };
            _entity = new DynamicTableEntity("agg1", "1", "etag",
                                             new Dictionary<string, EntityProperty>
                                                 {
                                                     {
                                                         "d1",
                                                         new EntityProperty(_existingData)
                                                     }
                                                 });
            _row = new BatchOperationRow(_entity, false);
        };

        private Because of = () => _dataLeft = _row.Write(_dataToWrite, 0);

        private It should_have_a_column = () => _row.Columns.Count().ShouldEqual(1);

        private It should_have_size_of_all_data = () => _row.Size.ShouldEqual(_dataToWrite.Length + _existingData.Length);

        private It should_be_dirty = () => _row.IsDirty.ShouldBeTrue();

        private It should_write_all_data = () => _dataLeft.Length.ShouldEqual(0);

        private static BatchOperationRow _row;
        private static byte[] _dataLeft;
        private static DynamicTableEntity _entity;
        private static byte[] _dataToWrite;
        private static byte[] _existingData;
    }
}