using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Writer
{
    public class when_creating_a_batch_operation_from_an_existing_writer
    {
        private Establish context = () =>
                                        {
                                            _streamName = "agg1";
                                            _dataToWrite = new byte[] { 4, 5, 6 };
                                            _existingData = new byte[] {1, 2, 3};
                                            _existingEntities = new []
                                                                    {
                                                                        new DynamicTableEntity(_streamName, "1", "etag",
                                                                             new Dictionary<string, EntityProperty>
                                                                                 {
                                                                                     {
                                                                                         "d1",
                                                                                         new EntityProperty(_existingData)
                                                                                     }
                                                                                 })
                                                                    };
                                            _writer = new BatchOperationWriter(_streamName, _existingEntities, false);
                                            _stream = new MemoryStream(_dataToWrite);
                                            _writer.Write(_stream);
                                        };

        private Because of = () => _batchOperation = _writer.ToBatchOperation();

        private It should_have_operations_in_batch = () => _batchOperation.ShouldNotBeEmpty();

        private It should_only_have_dirty_rows_in_batch = () => _batchOperation.Count.ShouldEqual(_writer.Rows.Count(r => r.IsDirty));

        private static BatchOperationWriter _writer;
        private static string _streamName;
        private static IEnumerable<DynamicTableEntity> _existingEntities;
        private static MemoryStream _stream;
        private static byte[] _existingData;
        private static byte[] _dataToWrite;
        private static TableBatchOperation _batchOperation;
    }
}