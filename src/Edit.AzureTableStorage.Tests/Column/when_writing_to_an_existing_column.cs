using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_writing_to_an_existing_column
    {
        private Establish context = () =>
        {
            _dataToWrite = new byte[] { 4, 5, 6 };
            _existingData = new byte[] { 1, 2, 3 };
            _column = new BatchOperationColumn(new EntityProperty(_existingData));
        };

        private Because of = () => _dataLeft = _column.Write(_dataToWrite, 0);

        private It should_have_the_existing_and_written_data =
            () => _column.Data.ShouldEqual(new byte[] {1, 2, 3, 4, 5, 6});

        private It should_have_a_size_of_the_existing_and_written_data =
            () => _column.Size.ShouldEqual(_existingData.Length + _dataToWrite.Length);

        private It should_write_all_data = () => _dataLeft.Length.ShouldEqual(0);

        private It should_be_dirty = () => _column.IsDirty.ShouldBeTrue();

        private static BatchOperationColumn _column;
        private static byte[] _dataToWrite;
        private static byte[] _existingData;
        private static byte[] _dataLeft;
    }
}