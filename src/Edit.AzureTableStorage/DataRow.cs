using System;
using System.Collections.Generic;
using System.IO;

namespace Edit.AzureTableStorage
{
    internal class DataRow : IDisposable
    {
        private readonly AppendOnlyStoreTableEntity _entity;
        private readonly IEnumerator<DataColumn> _columns;
        private DataColumn _currentColumn;
        private int _currentColumnNo = 0;
        private MemoryStream _memoryStream = null;
        private bool _isEmpty = true;

        public static int MaxSize
        {
            get { return ColumnsWrapper.NumberOfColumnsPerRow * DataColumn.MaxSize; }
        }

        public DataRow(int currentChunkNo, int currentRowNo)
        {
            _entity = new AppendOnlyStoreTableEntity
                {
                    FirstChunkNoWrittenToRow = currentChunkNo,
                    RowKey = currentRowNo.ToString()
                };
            _columns = new ColumnsWrapper(_entity).DataColumns.GetEnumerator();
            MoveNextColumn();
        }

        private bool MoveNextColumn()
        {
            if (_isFull)
            {
                return false;
            }
            if (_memoryStream != null)
            {
                _memoryStream.Dispose();
            }
            bool canMove = _columns.MoveNext();
            if (canMove)
            {
                _currentColumnNo++;
                _currentColumn = _columns.Current;
                _memoryStream = new MemoryStream();
                _noChunksInCurrentColumn = 0;
            }
            else
            {
                _currentColumn = null;
                _isFull = true;
            }
            return canMove;
        }

        private int _noChunksInCurrentColumn;
        private int _currentColumnSize;
        private bool _isFull = false;

        private void FlagCurrentColumnAsMultipleColumnsChunk()
        {
            _currentColumn.MarkAsMultipleColumnChunk();
        }

        private void FlagCurrentColumnAsLastPieceOfMultipleColumnsChunk()
        {
            _currentColumn.MarkAsLastPieceOfMultipleColumnChunk();
        }

        private bool WriteLargeChunkToMultipleColumns(byte[] result)
        {
            int resultSize = result.Length;
            if (!CanFitChunkLargerThanColumnSize(resultSize))
            {
                return false;
            }
            
            if (!IsCurrentColumnEmpty())
            {
                FlushChunks();
                MoveNextColumn();
            }

            int currentPosition = 0;
            while (currentPosition < resultSize)
            {
                int bytesToWrite = DataColumn.MaxSize;
                if (resultSize > currentPosition + bytesToWrite)
                {
                    FlagCurrentColumnAsMultipleColumnsChunk();
                }
                else
                {
                    bytesToWrite = resultSize - currentPosition;
                    FlagCurrentColumnAsLastPieceOfMultipleColumnsChunk();                    
                }
                _memoryStream.Write(result, currentPosition, bytesToWrite);
                currentPosition += bytesToWrite;
                FlushChunks();
                MoveNextColumn();
            }
            _currentColumnSize = 0;

            return true;
        }

        public bool WriteChunk(byte[] result)
        {
            if (_isFull)
            {
                return false;
            }
            int resultSize = result.Length;
            if (resultSize > DataRow.MaxSize)
            // Cannot handle single message being larger than one column. Could be fixed by allowing a message to expand to multiple columns and rows
            {
                throw new StorageSizeException("Messages larger than " + DataRow.MaxSize +
                                               " bytes is not supported");
            }
            _isEmpty = false;

            if (resultSize > DataColumn.MaxSize)
            {
                return WriteLargeChunkToMultipleColumns(result);
            }
            _currentColumnSize += resultSize;
            if (_currentColumnSize > DataColumn.MaxSize)
            {
                FlushChunks();
                if (!MoveNextColumn())
                {
                    return false;
                }
                _currentColumnSize = resultSize;
            }
            _noChunksInCurrentColumn++;
            _memoryStream.Write(result, 0, result.Length);
            return true;
        }

        private void FlushChunks()
        {
            if (_currentColumn != null && !IsEmpty())
            {
                var data = _memoryStream.ToArray();
                if (data.Length > 0)
                {
                    _currentColumn.Set(data);
                    _currentColumn.SetNumberOfChunks(_noChunksInCurrentColumn);
                }
            }
        }

        private bool IsCurrentColumnEmpty()
        {
            return _currentColumnSize == 0;
        }

        private bool IsEmpty()
        {
            return _isEmpty;
        }

        private int AvailableMaxMessageSize
        {
            get
            {
                if (IsEmpty())
                {
                    return MaxSize;
                }
                int currentColumnHasDataCompensation = IsCurrentColumnEmpty() ? 0 : 1; // If current column has data it will not be used when writing large messages (larger than one column)
                int noEmptyColumnsLeft = ColumnsWrapper.NumberOfColumnsPerRow - (_currentColumnNo - 1) - currentColumnHasDataCompensation;
                return noEmptyColumnsLeft*DataColumn.MaxSize;
            }
        }

        private bool CanFitChunkLargerThanColumnSize(int chunkSize)
        {
            if (_isFull)
            {
                return false;
            }
            return chunkSize <= AvailableMaxMessageSize;
        }

        public AppendOnlyStoreTableEntity CreateEntity()
        {
            FlushChunks();
            _entity.IsFull = _isFull;
            return _entity;
        }

        public void Dispose()
        {
            if (_memoryStream != null)
            {
                _memoryStream.Dispose();
            }
        }

    }
}
