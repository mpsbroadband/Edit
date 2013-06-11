using System;
using System.IO;

namespace Edit.AzureTableStorage
{
    internal class DataRow : IDisposable
    {
        private readonly AppendOnlyStoreDynamicTableEntity _entity;
        private DataColumn _currentColumn;
        private MemoryStream _memoryStream;
        private bool _isEmpty = true;
        private const int MaxSizeStorageEmulator = 300*1024; // Storage limit of emulator
        private const int MaxSizeProduction = 900*1024; // 900 KB. Max DB row size is 1 MB. We allow the data in the data columns to be 900 KB, leaving storage for other columns, column titles, etc

        public static int MaxDataSize
        {
            get
            {
                if (AzureTableStorageAppendOnlyStore.IsStorageEmulator)
                {
                    return MaxSizeStorageEmulator;
                }
                return MaxSizeProduction;
            }
        }

        public DataRow(int currentChunkNo, int currentRowNo)
        {
            _entity = new AppendOnlyStoreDynamicTableEntity
                {
                    FirstChunkNoWrittenToRow = currentChunkNo,
                    RowKey = currentRowNo.ToString()
                };
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
            bool canMove = _entity.MoveNextColumn();
            if (canMove)
            {
                _currentColumn = _entity.CurrentColumn;
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
        private bool _isFull;

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
            if (resultSize > MaxDataSize)
            // Cannot handle single message being larger than one column. Could be fixed by allowing a message to expand to multiple columns and rows
            {
                throw new StorageSizeException("Messages larger than " + MaxDataSize +
                                               " bytes is not supported");
            }
            if (!CanFitChunk(resultSize))
            {
                _isFull = true;
                return false;
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
                    return MaxDataSize;
                }
                return MaxDataSize - _entity.DataSize;
            }
        }

        private bool CanFitChunk(int chunkSize)
        {
            if (_isFull)
            {
                return false;
            }
            return chunkSize <= AvailableMaxMessageSize;
        }

        public AppendOnlyStoreDynamicTableEntity CreateEntity()
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
