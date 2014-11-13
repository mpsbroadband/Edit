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
        private readonly List<BatchOperationRow> _rows;

        public BatchOperationWriter(string streamName, IEnumerable<DynamicTableEntity> existingEntities)
        {
            _streamName = streamName;
            _rows = existingEntities.Select(e => new BatchOperationRow(e)).ToList();
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
            
            foreach (var row in Rows.Where(r => r.IsDirty))
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
                        _rows.Add(new BatchOperationRow(StreamName, _rows.Count));
                    }
                }
            }
        }
    }
}