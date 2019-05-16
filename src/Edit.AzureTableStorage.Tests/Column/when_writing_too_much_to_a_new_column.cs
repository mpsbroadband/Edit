using System;
using AzureApi.Storage.Table;
using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_writing_too_much_to_a_new_column
    {
        private Establish context = () =>
        {
            _column = new BatchOperationColumn();
            _maxData = new byte[_column.MaxSize];
            _overflowData = new byte[] {1, 2, 3};

            new Random().NextBytes(_maxData);

            _data = new byte[_maxData.Length + _overflowData.Length];
            _maxData.CopyTo(_data, 0);
            Array.Copy(_overflowData, 0, _data, _maxData.Length, _overflowData.Length);
        };

        private Because of = () => _dataLeft = _column.Write(_data, 0);

        private It should_have_the_max_written_data = () => _column.Data.ShouldEqual(_maxData);

        private It should_have_a_size_of_max_size = () => _column.Size.ShouldEqual(_column.MaxSize);

        private It should_be_dirty = () => _column.IsDirty.ShouldBeTrue();

        private It should_return_overflow_data = () => _dataLeft.ShouldEqual(_overflowData);

        private static BatchOperationColumn _column;
        private static byte[] _maxData;
        private static byte[] _dataLeft;
        private static byte[] _overflowData;
        private static byte[] _data;
    }
}