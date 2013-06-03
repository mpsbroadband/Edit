using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal class ColumnsWrapper
    {
        private readonly AppendOnlyStoreTableEntity _entity;

        public ColumnsWrapper(AppendOnlyStoreTableEntity entity)
        {
            _entity = entity;
        }

        internal IEnumerable<DataColumn> DataColumns
        {
            get 
            {
                yield return new DataColumn((data) => _entity.Data = data, () => _entity.Data, (noChunks) => _entity.NoChunksInData = noChunks, () => _entity.NoChunksInData);
                yield return new DataColumn((data) => _entity.Data2 = data, () => _entity.Data2, (noChunks) => _entity.NoChunksInData2 = noChunks, () => _entity.NoChunksInData2);
                yield return new DataColumn((data) => _entity.Data3 = data, () => _entity.Data3, (noChunks) => _entity.NoChunksInData3 = noChunks, () => _entity.NoChunksInData3);
                yield return new DataColumn((data) => _entity.Data4 = data, () => _entity.Data4, (noChunks) => _entity.NoChunksInData4 = noChunks, () => _entity.NoChunksInData4);
                yield return new DataColumn((data) => _entity.Data5 = data, () => _entity.Data5, (noChunks) => _entity.NoChunksInData5 = noChunks, () => _entity.NoChunksInData5);
                yield return new DataColumn((data) => _entity.Data6 = data, () => _entity.Data6, (noChunks) => _entity.NoChunksInData6 = noChunks, () => _entity.NoChunksInData6);
                yield return new DataColumn((data) => _entity.Data7 = data, () => _entity.Data7, (noChunks) => _entity.NoChunksInData7 = noChunks, () => _entity.NoChunksInData7);
                yield return new DataColumn((data) => _entity.Data8 = data, () => _entity.Data8, (noChunks) => _entity.NoChunksInData8 = noChunks, () => _entity.NoChunksInData8);
                yield return new DataColumn((data) => _entity.Data9 = data, () => _entity.Data9, (noChunks) => _entity.NoChunksInData9 = noChunks, () => _entity.NoChunksInData9);
                yield return new DataColumn((data) => _entity.Data10 = data, () => _entity.Data10, (noChunks) => _entity.NoChunksInData10 = noChunks, () => _entity.NoChunksInData10);
                yield return new DataColumn((data) => _entity.Data11 = data, () => _entity.Data11, (noChunks) => _entity.NoChunksInData11 = noChunks, () => _entity.NoChunksInData11);
                yield return new DataColumn((data) => _entity.Data12 = data, () => _entity.Data12, (noChunks) => _entity.NoChunksInData12 = noChunks, () => _entity.NoChunksInData12);
                yield return new DataColumn((data) => _entity.Data13 = data, () => _entity.Data13, (noChunks) => _entity.NoChunksInData13 = noChunks, () => _entity.NoChunksInData13);
            }
        }
    }
}
