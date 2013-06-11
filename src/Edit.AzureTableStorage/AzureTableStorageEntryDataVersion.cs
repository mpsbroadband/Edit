using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal class AzureTableStorageEntryDataVersion : IStoredDataVersion
    {
        internal String Version { get; set; }

        internal int LastRowKey { get; set; }
        
        internal int FirstChunkNoOfRow { get; set; }

        public override string ToString()
        {
            return Version;
        }
    }
}
