using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Writer
{
    public class when_writing_to_a_new_writer
    {
        private Establish context = () =>
                                        {
                                            _streamName = "agg1";
                                            _existingEntities = new DynamicTableEntity[0];
                                            _writer = new BatchOperationWriter(_streamName, "test", _existingEntities, false);
                                            
                                            var data = new byte[1024 * 1024 * 2];

                                            new Random().NextBytes(data);

                                            _stream = new MemoryStream(data);
                                        };

        private Because of = () => _writer.Write(_stream);

        private It should_have_a_stream_name = () => _writer.StreamName.ShouldEqual(_streamName);

        private It should_have_rows = () => _writer.Rows.ShouldNotBeEmpty();

        private It should_write_all_data = () => _writer.Rows.SelectMany(r => r.Columns.Select(c => c.Data)).SelectMany(d => d).ToArray().ShouldEqual(_stream.ToArray());

        private static BatchOperationWriter _writer;
        private static string _streamName;
        private static IEnumerable<DynamicTableEntity> _existingEntities;
        private static MemoryStream _stream;
    }
}