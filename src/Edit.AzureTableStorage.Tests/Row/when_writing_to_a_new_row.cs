using System.Linq;
using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_writing_to_a_new_row
    {
        private Establish context = () =>
        {
            _row = new BatchOperationRow("agg1", 1, false);
            _data = new byte[] { 1, 2, 3 };
        };

        private Because of = () => _dataLeft = _row.Write(_data, 0);

        private It should_have_a_column = () => _row.Columns.Count().ShouldEqual(1);

        private It should_have_size_of_data = () => _row.Size.ShouldEqual(_data.Length);

        private It should_be_dirty = () => _row.IsDirty.ShouldBeTrue();

        private It should_write_all_data = () => _dataLeft.Length.ShouldEqual(0);

        private static BatchOperationRow _row;
        private static byte[] _data;
        private static byte[] _dataLeft;
    }
}