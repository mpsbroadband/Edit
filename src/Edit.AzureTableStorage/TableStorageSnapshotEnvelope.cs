namespace Edit.AzureTableStorage
{
    public class TableStorageSnapshotEnvelope<T> : ISnapshotEnvelope<T>
    {
        public TableStorageSnapshotEnvelope(string partitionKey, string rowKey, string column, int position, T snapshot)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Column = column;
            Position = position;
            Snapshot = snapshot;
        }

        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }
        public string Column { get; private set; }
        public int Position { get; private set; }
        public T Snapshot { get; private set; }
    }
}