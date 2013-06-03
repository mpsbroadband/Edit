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

        internal IEnumerable<Func<byte[]>> DataColumns
        {
            get 
            {
                yield return () => _entity.Data;
                yield return () => _entity.Data2;
                yield return () => _entity.Data3;
                yield return () => _entity.Data4;
                yield return () => _entity.Data5;
                yield return () => _entity.Data6;
                yield return () => _entity.Data7;
                yield return () => _entity.Data8;
                yield return () => _entity.Data9;
                yield return () => _entity.Data10;
                yield return () => _entity.Data11;
                yield return () => _entity.Data12;
                yield return () => _entity.Data13; 
            }
        }
    }
}
