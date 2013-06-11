using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    public class DataColumn
    {
        private readonly Action<byte[]> _setter;
        private readonly Func<byte[]> _getter;
        private readonly Action<int> _noChunksSetter;
        private readonly Func<int> _noChunksGetter;
        private bool _containsMultipleColumnsChunk;

        private const int MultipleColumnsChunk = -1;
        private const int LastPieceOfMultipleColumnsChunk = -2;

        public const int MaxSize = 65536; // 64KB

        public DataColumn(Action<byte[]> setter, Func<byte[]> getter, Action<int> noChunksSetter, Func<int> noChunksGetter)
        {
            _setter = setter;
            _getter = getter;
            _noChunksSetter = noChunksSetter;
            _noChunksGetter = noChunksGetter;
        }

        public void Set(byte[] data)
        {
            _setter(data);
        }

        public byte[] Get()
        {
            return _getter();
        }

        public void SetNumberOfChunks(int noChunks)
        {
            if (!_containsMultipleColumnsChunk)
            {
                _noChunksSetter(noChunks);
            }
        }

        public int GetNumberOfChunks()
        {
            return _noChunksGetter();
        }

        public void MarkAsMultipleColumnChunk()
        {
            SetNumberOfChunks(MultipleColumnsChunk);
            _containsMultipleColumnsChunk = true;
        }

        public void MarkAsLastPieceOfMultipleColumnChunk()
        {
            SetNumberOfChunks(LastPieceOfMultipleColumnsChunk);
            _containsMultipleColumnsChunk = true;
        }

        public bool ContainsMultipleColumnsChunk()
        {
            int chunkNo = GetNumberOfChunks();
            return chunkNo == MultipleColumnsChunk
                || chunkNo == LastPieceOfMultipleColumnsChunk;
        }

        public bool IsLastPieceOfMultipleColumnsChunk()
        {
            return GetNumberOfChunks() == LastPieceOfMultipleColumnsChunk;
        }
    }
}
