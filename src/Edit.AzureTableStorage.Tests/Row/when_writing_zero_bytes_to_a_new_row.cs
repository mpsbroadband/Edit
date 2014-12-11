using System.Linq;
using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_writing_zero_bytes_to_a_new_row
    {
        private Establish context = () =>
        {
            _row = new BatchOperationRow("agg1", 1, false);
            _data = new byte[0];
        };

        private Because of = () => _row.Write(_data, 0);

        private It should_have_a_column = () => _row.Columns.Count().ShouldEqual(1);

        private It should_have_size_of_data = () => _row.Size.ShouldEqual(_data.Length);

        private It should_be_dirty = () => _row.IsDirty.ShouldBeTrue();

        private static BatchOperationRow _row;
        private static byte[] _data;
    }
}