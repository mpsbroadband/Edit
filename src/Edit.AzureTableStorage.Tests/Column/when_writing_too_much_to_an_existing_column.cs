using System;
using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_writing_too_much_to_an_existing_column
    {
        private Establish context = () =>
        {
            _existingData = new byte[] { 1, 2, 3 };
            _column = new BatchOperationColumn(new EntityProperty(_existingData));
            _remainingData = new byte[_column.MaxSize - _column.Size];
            _overflowData = new byte[] {4, 5, 6};

            new Random().NextBytes(_remainingData);

            _data = new byte[_remainingData.Length + _overflowData.Length];
            _remainingData.CopyTo(_data, 0);
            Array.Copy(_overflowData, 0, _data, _remainingData.Length, _overflowData.Length);

            _writtenData = new byte[_existingData.Length + _remainingData.Length];
            _existingData.CopyTo(_writtenData, 0);
            Array.Copy(_remainingData, 0, _writtenData, _existingData.Length, _remainingData.Length);
        };

        private Because of = () => _dataLeft = _column.Write(_data, 0);

        private It should_have_the_max_written_data = () => _column.Data.ShouldEqual(_writtenData);

        private It should_have_a_size_of_max_size = () => _column.Size.ShouldEqual(_column.MaxSize);

        private It should_be_dirty = () => _column.IsDirty.ShouldBeTrue();

        private It should_return_overflow_data = () => _dataLeft.ShouldEqual(_overflowData);

        private static BatchOperationColumn _column;
        private static byte[] _remainingData;
        private static byte[] _dataLeft;
        private static byte[] _overflowData;
        private static byte[] _data;
        private static byte[] _existingData;
        private static byte[] _writtenData;
    }
}