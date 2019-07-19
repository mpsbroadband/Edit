using AzureApi.Storage.Table;
using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_writing_to_a_new_column
    {
        private Establish context = () =>
        {
            _column = new BatchOperationColumn();
            _data = new byte[] {1, 2, 3};
        };

        private Because of = () => _dataLeft = _column.Write(_data, 0);

        private It should_have_the_written_data = () => _column.Data.ShouldEqual(_data);

        private It should_have_a_size_of_the_written_data = () => _column.Size.ShouldEqual(_data.Length);

        private It should_be_dirty = () => _column.IsDirty.ShouldBeTrue();

        private It should_write_all_data = () => _dataLeft.Length.ShouldEqual(0);

        private static BatchOperationColumn _column;
        private static byte[] _data;
        private static byte[] _dataLeft;
    }
}