using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal class EntitySort : IComparer<AppendOnlyStoreDynamicTableEntity>
    {
        public int Compare(AppendOnlyStoreDynamicTableEntity x, AppendOnlyStoreDynamicTableEntity y)
        {
            return int.Parse(x.RowKey).CompareTo(int.Parse(y.RowKey));
        }
    }
}
