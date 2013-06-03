using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public sealed class AppendOnlyStoreTableEntity : TableEntity
    {
        public byte[] Data { get; set; }
        public byte[] Data2 { get; set; }
        public byte[] Data3 { get; set; }
        public byte[] Data4 { get; set; }
        public byte[] Data5 { get; set; }
        public byte[] Data6 { get; set; }
        public byte[] Data7 { get; set; }
        public byte[] Data8 { get; set; }
        public byte[] Data9 { get; set; }
        public byte[] Data10 { get; set; }
        public byte[] Data11 { get; set; }
        public byte[] Data12 { get; set; }
        public byte[] Data13 { get; set; }
        public int NoChunksInData { get; set; }
        public int NoChunksInData2 { get; set; }
        public int NoChunksInData3 { get; set; }
        public int NoChunksInData4 { get; set; }
        public int NoChunksInData5 { get; set; }
        public int NoChunksInData6 { get; set; }
        public int NoChunksInData7 { get; set; }
        public int NoChunksInData8 { get; set; }
        public int NoChunksInData9 { get; set; }
        public int NoChunksInData10 { get; set; }
        public int NoChunksInData11 { get; set; }
        public int NoChunksInData12 { get; set; }
        public int NoChunksInData13 { get; set; }
        public bool IsFull { get; set; }
    }
}