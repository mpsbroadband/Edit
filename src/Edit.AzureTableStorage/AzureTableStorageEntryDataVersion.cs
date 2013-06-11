using System;

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
