namespace Edit
{
    public sealed class Record
    {
        public byte[] Data { get; set; }
        public string StreamVersion { get; set; }

        public Record(byte[] data, string streamVersion)
        {
            Data = data;
            StreamVersion = streamVersion;
        }
    }
}