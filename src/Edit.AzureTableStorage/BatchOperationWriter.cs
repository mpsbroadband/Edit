using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class BatchOperationWriter
    {
        private readonly string _streamName;
        private readonly string _sequencePrefix;
        private readonly bool _developmentStorage;
        private readonly IList<BatchOperationRow> _rows;

        public BatchOperationWriter(string streamName, string sequencePrefix, IEnumerable<DynamicTableEntity> existingEntities, bool developmentStorage)
        {
            _streamName = streamName;
            _sequencePrefix = sequencePrefix;
            _developmentStorage = developmentStorage;
            _rows = existingEntities.Select(e => new BatchOperationRow(e, developmentStorage)).ToList();

            if (_rows.Count == 0)
            {
                _rows.Add(new BatchOperationRow(_streamName, _sequencePrefix, 0, _developmentStorage));
            }
        }

        public string StreamName
        {
            get { return _streamName; }
        }

        public IEnumerable<BatchOperationRow> Rows
        {
            get { return new ReadOnlyCollection<BatchOperationRow>(_rows); }
        }

        public TableBatchOperation ToBatchOperation()
        {
            var batch = new TableBatchOperation();
            var last = Rows.Last();

            foreach (var row in Rows.Where(r => r.IsDirty || r == last))
            {
                batch.Add(row.ToTableOperation());
            }

            return batch;
        }

        public void Write(Stream stream)
        {
            var buffer = new byte[4096];
            int read;

            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var data = new byte[read];
                Array.Copy(buffer, 0, data, 0, data.Length);

                while (data.Length > 0)
                {
                    var row = Rows.SingleOrDefault(r => r.MaxSize > r.Size);

                    if (row != null)
                    {
                        data = row.Write(data, 0);
                    }
                    else
                    {
                        _rows.Add(new BatchOperationRow(StreamName, _sequencePrefix, _rows.Count, _developmentStorage));
                    }
                }
            }
        }
    }
}