namespace Edit
{
    public interface ISnapshotEnvelope<T>
    {
        T Snapshot { get; }
    }
}