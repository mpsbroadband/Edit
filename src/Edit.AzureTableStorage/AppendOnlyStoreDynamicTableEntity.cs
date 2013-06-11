using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class AppendOnlyStoreDynamicTableEntity
    {
        private readonly DynamicTableEntity _entity;
        private int _noColumns;
        private int _currentColumnNo;
        private readonly List<DataColumn> _columns = new List<DataColumn>();
        private readonly bool _isNew;
        private const String FirstChunkNoWrittenToRowColumn = "FirstChunkNo";
        private const String IsFullColumn = "IsFull";
        private const String NoDataColumns = "NoColumns";
        private DataColumn _currentColumn;

        public static AppendOnlyStoreDynamicTableEntity Parse(ITableEntity entity)
        {
            return new AppendOnlyStoreDynamicTableEntity((DynamicTableEntity)entity, false);
        }

        public AppendOnlyStoreDynamicTableEntity() : this(new DynamicTableEntity(), true) { }

        private AppendOnlyStoreDynamicTableEntity(DynamicTableEntity entity, bool isNew)
        {
            _entity = entity;
            _isNew = isNew;
            if (!_isNew)
            {
                _noColumns = NoColumns;
                CreateAllColumns();
            }
        }

        private void CreateAllColumns()
        {
            for (var i = _currentColumnNo; i < _noColumns; i++)
            {
                CreateColumn(i);
            }
        }

        private int NoColumns
        {
            get { return ReadInt(NoDataColumns); }
            set { _entity.Properties[NoDataColumns] = new EntityProperty(value); }
        }

        private int ReadInt(String columnName)
        {
            EntityProperty entityProperty;
            if (_entity.Properties.TryGetValue(columnName, out entityProperty))
            {
                int? val = entityProperty.Int32Value;
                return val == null ? 0 : val.Value;
            }
            return 0;
        }

        private void UpdateNumberOfDataColums(int noColumns)
        {
            _noColumns = noColumns;
            NoColumns = _noColumns;
        }

        private DataColumn CreateColumn(int columnNo)
        {
            String dataColumn = "Data" + columnNo;
            String noChunksColumn = "NoChunks" + columnNo;
            var column = new DataColumn(
                data => SetData(data, dataColumn),
                () => GetData(dataColumn),
                noChunks => SetNumberOfChunks(noChunks, noChunksColumn),
                () => GetNumberOfChunks(noChunksColumn));

            _columns.Add(column);
            return column;
        }

        public bool MoveNextColumn()
        {
            if (!_isNew && _currentColumnNo >= _noColumns)
            {
                return false;
            }
            var column = CreateColumn(_currentColumnNo);

            _currentColumnNo++;
            if (_isNew)
            {
                UpdateNumberOfDataColums(_noColumns + 1);
            }
            _currentColumn = column;
            return true;
        }

        public DataColumn CurrentColumn
        {
            get { return _currentColumn; }
        }

        private void SetData(byte[] data, String columnName)
        {
            _entity.Properties[columnName] = new EntityProperty(data);
        }

        private byte[] GetData(String columnName)
        {
            EntityProperty entityProperty;
            if (_entity.Properties.TryGetValue(columnName, out entityProperty))
            {
                return entityProperty.BinaryValue;
            }
            return null;
        }

        private void SetNumberOfChunks(int noChunks, String columnName)
        {
            _entity.Properties[columnName] = new EntityProperty(noChunks);
        }

        private int GetNumberOfChunks(String columnName)
        {
            return ReadInt(columnName);
        }

        public int DataSize
        {
            get
            {
                return _columns.Select(dataColumn => dataColumn.Get()).Where(data => data != null).Sum(data => data.Length);
            }
        }

        public bool IsFull
        {
            get
            {
                EntityProperty entityProperty;
                if (_entity.Properties.TryGetValue(IsFullColumn, out entityProperty))
                {
                    bool? val = entityProperty.BooleanValue;
                    return val != null && val.Value;
                }
                return false;
            }
            set { _entity.Properties[IsFullColumn] = new EntityProperty(value); }
        }

        public int FirstChunkNoWrittenToRow
        {
            get { return ReadInt(FirstChunkNoWrittenToRowColumn); }
            set { _entity.Properties[FirstChunkNoWrittenToRowColumn] = new EntityProperty(value); }
        }

        public int NumberOfDataColumns 
        { 
            get { return _noColumns; }
        }

        public ITableEntity Entity
        {
            get { return _entity; }
        }

        public String RowKey
        {
            set { _entity.RowKey = value; }
            get { return _entity.RowKey; }
        }

        public String PartitionKey
        {
            set { _entity.PartitionKey = value; }
            get { return _entity.PartitionKey; }
        }

        public String ETag
        {
            set { _entity.ETag = value; }
            get { return _entity.ETag; }
        }
    }
}
