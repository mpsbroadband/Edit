namespace Edit
{
    public interface ISnapshotEnvelope<out T>
    {
        T Snapshot { get; }
    }
}