namespace Edit
{
    public sealed class Record
    {
        public byte[] Data { get; set; }
        public IStoredDataVersion StreamVersion { get; set; }

        public Record(byte[] data, IStoredDataVersion streamVersion)
        {
            Data = data;
            StreamVersion = streamVersion;
        }
    }
}