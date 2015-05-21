using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class BatchOperationRow
    {
        private const int MaxRowSizeEmulator = 300 * 1024;
        private const int MaxRowSize = 900 * 1024;

        private readonly bool _developmentStorage;
        private readonly IList<BatchOperationColumn> _columns;
        private readonly int _sequence;
        private readonly string _streamName;
        private readonly string _etag;
        private readonly string _sequencePrefix;

        public BatchOperationRow(string streamName, string sequencePrefix, long sequence, bool developmentStorage)
            : this(new DynamicTableEntity(streamName,
                    FormatRowKey(sequencePrefix, sequence), null,
                    new Dictionary<string, EntityProperty>()), developmentStorage)
        {
            _columns = new List<BatchOperationColumn> {new BatchOperationColumn()};
        }

        public BatchOperationRow(DynamicTableEntity entity, bool developmentStorage)
        {
            _developmentStorage = developmentStorage;
            _streamName = entity.PartitionKey;
            _etag = entity.ETag;
            _columns = entity.Properties.OrderByAlphaNumeric(e => e.Key).Select(p => new BatchOperationColumn(p.Value)).ToList();

            var match = Regex.Match(entity.RowKey, @"(.*)-(\d+)");

            if (!match.Success)
            {
                throw new ArgumentException(string.Format("{0} is not a valid sequence row key.", entity.RowKey), "entity");
            }

            _sequencePrefix = match.Groups[1].Value;
            _sequence = int.Parse(match.Groups[2].Value);
        }

        public string StreamName
        {
            get { return _streamName; }
        }

        public string SequencePrefix
        {
            get { return _sequencePrefix; }
        }

        public long Sequence
        {
            get { return _sequence; }
        }

        public string ETag
        {
            get { return _etag; }
        }

        public int Size
        {
            get { return _columns.Sum(c => c.Size); }
        }

        public int MaxSize
        {
            get { return _developmentStorage ? MaxRowSizeEmulator : MaxRowSize; }
        }

        public bool IsDirty
        {
            get { return _columns.Any(c => c.IsDirty); }
        }

        public IEnumerable<BatchOperationColumn> Columns
        {
            get { return new ReadOnlyCollection<BatchOperationColumn>(_columns); }
        }

        public TableOperation ToTableOperation()
        {
            var columnSequence = 0;
            var entity = new DynamicTableEntity(StreamName,
                FormatRowKey(SequencePrefix, Sequence), ETag,
                Columns.ToDictionary(c => FormatColumnName(columnSequence++), c => c.ToProperty()));
            
            if (string.IsNullOrEmpty(ETag))
            {
                return TableOperation.Insert(entity);
            }

            return TableOperation.Replace(entity);
        }

        public byte[] Write(byte[] buffer, int offset)
        {
            var bufferLength = buffer.Length - offset;
            var rowSizeLeft = Math.Min(bufferLength, MaxSize - Size);
            var rowDataLeft = new byte[rowSizeLeft];

            Array.Copy(buffer, offset, rowDataLeft, 0, rowDataLeft.Length);

            while (rowDataLeft.Length > 0)
            {
                var column = _columns.SingleOrDefault(c => c.MaxSize > c.Size);

                if (column != null)
                {
                    rowDataLeft = column.Write(rowDataLeft, 0);
                }
                else
                {
                    _columns.Add(new BatchOperationColumn());
                }
            }

            var remainder = new byte[bufferLength - rowSizeLeft];
            var startIndex = offset + rowSizeLeft;

            Array.Copy(buffer, startIndex, remainder, 0, remainder.Length);

            return remainder;
        }

        public static string FormatRowKey(string sequencePrefix, long sequence)
        {
            return string.Concat(sequencePrefix, "-", sequence.ToString(CultureInfo.InvariantCulture));
        }

        public static string FormatColumnName(long sequence)
        {
            return string.Format("d{0}", sequence);
        }
    }
}