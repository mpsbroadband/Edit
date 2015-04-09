using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public class TableStorageVersion : IVersion
    {
        public TableStorageVersion(string partitionKey, IEnumerable<DynamicTableEntity> entities)
        {
            Entities = new ReadOnlyCollection<DynamicTableEntity>(entities.ToList());
            ETag = Entities.Select(e => e.ETag).LastOrDefault() ?? string.Empty;
            PartitionKey = partitionKey;

            if (Entities.Any())
            {
                var last = Entities.Last();
                RowKey = last.RowKey;
                Column = last.Properties.OrderBy(p => p.Key).Last().Key;
                Position = last.Properties.OrderBy(p => p.Key).Last().Value.BinaryValue.Length;
            }
            else
            {
                RowKey = BatchOperationRow.FormatRowKey(TableStorageStreamStore.StreamSequencePrefix, 0);
                Column = BatchOperationRow.FormatColumnName(0);
                Position = 0;
            }
        }

        public IEnumerable<DynamicTableEntity> Entities { get; private set; }
        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }
        public string Column { get; private set; }
        public int Position { get; private set; }
        public string ETag { get; private set; }

        public override string ToString()
        {
            return ETag;
        }

        protected bool Equals(TableStorageVersion other)
        {
            return string.Equals(PartitionKey, other.PartitionKey) && string.Equals(RowKey, other.RowKey) &&
                   string.Equals(Column, other.Column) && Position == other.Position && string.Equals(ETag, other.ETag);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((TableStorageVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PartitionKey != null ? PartitionKey.GetHashCode() : 0);

                hashCode = (hashCode*397) ^ (RowKey != null ? RowKey.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Column != null ? Column.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Position;
                hashCode = (hashCode*397) ^ (ETag != null ? ETag.GetHashCode() : 0);

                return hashCode;
            }
        }
    }
}