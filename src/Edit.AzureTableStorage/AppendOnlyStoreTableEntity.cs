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
        public Guid?[] IdsInData { get; set; }
        public Guid?[] IdsInData2 { get; set; }
        public Guid?[] IdsInData3 { get; set; }
        public Guid?[] IdsInData4 { get; set; }
        public Guid?[] IdsInData5 { get; set; }
        public Guid?[] IdsInData6 { get; set; }
        public Guid?[] IdsInData7 { get; set; }
        public Guid?[] IdsInData8 { get; set; }
        public Guid?[] IdsInData9 { get; set; }
        public Guid?[] IdsInData10 { get; set; }
        public Guid?[] IdsInData11 { get; set; }
        public Guid?[] IdsInData12 { get; set; }
        public Guid?[] IdsInData13 { get; set; }
        public bool IsFull { get; set; }
    }
}