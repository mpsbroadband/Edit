using System.Collections.Generic;

namespace Edit.AzureTableStorage
{
    public interface IChunkSerializer
    {
        byte[] Write<T>(T frame) where T : class;

        IEnumerable<T> Read<T>(byte[] dataFrames) where T : class;
    }
}
