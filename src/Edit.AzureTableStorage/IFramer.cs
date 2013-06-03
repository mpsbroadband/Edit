using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    public interface IFramer
    {
        IEnumerable<T> Read<T>(AppendOnlyStoreTableEntity entity) where T : class;
        AppendOnlyStoreTableEntity Write<T>(IEnumerable<T> frames) where T : class;
    }
}
