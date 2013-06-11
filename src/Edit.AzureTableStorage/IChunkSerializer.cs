using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    public interface IChunkSerializer
    {
        byte[] Write<T>(T frame) where T : class;

        IEnumerable<T> Read<T>(byte[] dataFrames) where T : class;
    }
}
