using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class BatchOperationColumn
    {
        private const int MaxColumnSize = 64 * 1024;

        private readonly byte[] _originalData;
        private byte[] _data;

        public BatchOperationColumn() : this(new EntityProperty(new byte[0]))
        {   
        }

        public BatchOperationColumn(EntityProperty property)
        {
            _originalData = (byte[])(property.BinaryValue ?? new byte[0]).Clone();
            _data = (byte[])_originalData.Clone();
        }

        public int Size
        {
            get { return _data.Length; }
        }

        public int MaxSize
        {
            get { return MaxColumnSize; }
        }

        public byte[] Data
        {
            get { return (byte[])_data.Clone(); }
        }

        public bool IsDirty
        {
            get { return !_originalData.SequenceEqual(_data); }
        }

        public EntityProperty ToProperty()
        {
            return new EntityProperty(Data);
        }

        public byte[] Write(byte[] buffer, int offset)
        {
            var bufferLength = buffer.Length - offset;
            var bufferData = new byte[Math.Min(MaxColumnSize - Size, bufferLength)];
            var newData = new byte[Size + bufferData.Length];

            Array.Copy(buffer, offset, bufferData, 0, bufferData.Length);
            Array.Copy(Data, newData, Data.Length);
            Array.Copy(bufferData, 0, newData, Data.Length, bufferData.Length);

            _data = newData;

            if (bufferData.Length < bufferLength)
            {
                var remainder = new byte[bufferLength - bufferData.Length];
                Array.Copy(buffer, offset + bufferData.Length, remainder, 0, remainder.Length);
                return remainder;
            }
            else
            {
                return new byte[0];
            }
        }
    }
}