using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Writer
{
    public class when_writing_to_an_existing_writer
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
                                        };

        private Because of = () => _writer.Write(_stream);

        private It should_have_rows = () => _writer.Rows.ShouldNotBeEmpty();

        private It should_write_all_data = () => _writer.Rows.SelectMany(r => r.Columns.Select(c => c.Data)).SelectMany(d => d).ToArray().ShouldEqual(_existingData.Union(_dataToWrite));

        private static BatchOperationWriter _writer;
        private static string _streamName;
        private static IEnumerable<DynamicTableEntity> _existingEntities;
        private static MemoryStream _stream;
        private static byte[] _existingData;
        private static byte[] _dataToWrite;
    }
}