using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Writer
{
    public class when_creating_a_batch_operation_from_a_zero_length_writer
    {
        private Establish context = () =>
                                        {
                                            _streamName = "agg1";
                                            _existingEntities = new DynamicTableEntity[0];
                                            _writer = new BatchOperationWriter(_streamName, _existingEntities, false);
                                            
                                            var data = new byte[0];

                                            _stream = new MemoryStream(data);
                                            _writer.Write(_stream);
                                        };

        private Because of = () => _batchOperation = _writer.ToBatchOperation();

        private It should_have_a_single_operation_in_batch = () => _batchOperation.Count.ShouldEqual(1);

        private It should_only_have_dirty_rows = () => _writer.Rows.All(r => r.IsDirty).ShouldBeTrue();

        private static BatchOperationWriter _writer;
        private static string _streamName;
        private static IEnumerable<DynamicTableEntity> _existingEntities;
        private static MemoryStream _stream;
        private static TableBatchOperation _batchOperation;
    }
}