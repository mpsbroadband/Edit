using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Writer
{
    public class when_creating_a_batch_operation_from_an_existing_writer_with_no_changes
    {
        private Establish context = () =>
                                        {
                                            _streamName = "agg1";
                                            _dataToWrite = new byte[0];
                                            _existingData = new byte[] {1, 2, 3};
                                            _existingEntities = new []
                                                                    {
                                                                        new DynamicTableEntity(_streamName, "test-1", "etag1",
                                                                             new Dictionary<string, EntityProperty>
                                                                                 {
                                                                                     {
                                                                                         "d1",
                                                                                         new EntityProperty(_existingData)
                                                                                     }
                                                                                 }),
                                                                        new DynamicTableEntity(_streamName, "test-2", "etag2",
                                                                             new Dictionary<string, EntityProperty>
                                                                                 {
                                                                                     {
                                                                                         "d1",
                                                                                         new EntityProperty(_existingData)
                                                                                     }
                                                                                 })
                                                                    };
                                            _writer = new BatchOperationWriter(_streamName, "test", _existingEntities, false);
                                            _stream = new MemoryStream(_dataToWrite);
                                            _writer.Write(_stream);
                                        };

        private Because of = () => _batchOperation = _writer.ToBatchOperation();

        private It should_have_operations_in_batch = () => _batchOperation.ShouldNotBeEmpty();

        private It should_only_have_non_dirty_rows = () => _writer.Rows.All(r => !r.IsDirty).ShouldBeTrue();

        private static BatchOperationWriter _writer;
        private static string _streamName;
        private static IEnumerable<DynamicTableEntity> _existingEntities;
        private static MemoryStream _stream;
        private static byte[] _existingData;
        private static byte[] _dataToWrite;
        private static TableBatchOperation _batchOperation;
    }
}