using System;
using System.Linq;
using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_writing_too_much_to_a_new_row
    {
        private Establish context = () =>
        {
            _row = new BatchOperationRow("agg1", "test", 1, false);
            _maxData = new byte[_row.MaxSize];
            _overflowData = new byte[] {1, 2, 3};
            _data = new byte[_maxData.Length + _overflowData.Length];

            new Random().NextBytes(_maxData);

            _maxData.CopyTo(_data, 0);
            Array.Copy(_overflowData, 0, _data, _maxData.Length, _overflowData.Length);
        };

        private Because of = () => _dataLeft = _row.Write(_data, 0);

        private It should_have_columns = () => _row.Columns.ShouldNotBeEmpty();

        private It should_have_size_of_max_data = () => _row.Size.ShouldEqual(_maxData.Length);

        private It should_be_dirty = () => _row.IsDirty.ShouldBeTrue();

        private It should_return_overflow_data = () => _dataLeft.ShouldEqual(_overflowData);

        private static BatchOperationRow _row;
        private static byte[] _maxData;
        private static byte[] _dataLeft;
        private static byte[] _overflowData;
        private static byte[] _data;
    }
}