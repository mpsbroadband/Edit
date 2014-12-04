using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Writer
{
    public class when_writing_zero_bytes_to_a_new_writer
    {
        private Establish context = () =>
                                        {
                                            _streamName = "agg1";
                                            _existingEntities = new DynamicTableEntity[0];
                                            _writer = new BatchOperationWriter(_streamName, _existingEntities, false);
                                            
                                            var data = new byte[0];

                                            _stream = new MemoryStream(data);
                                        };

        private Because of = () => _writer.Write(_stream);

        private It should_have_a_stream_name = () => _writer.StreamName.ShouldEqual(_streamName);

        private It should_have_rows = () => _writer.Rows.ShouldNotBeEmpty();

        private static BatchOperationWriter _writer;
        private static string _streamName;
        private static IEnumerable<DynamicTableEntity> _existingEntities;
        private static MemoryStream _stream;
    }
}