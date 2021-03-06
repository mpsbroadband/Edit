﻿using System.Collections.Generic;
using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_creating_a_row_from_an_existing_entity
    {
        private Because of = () =>
        {
            _entity = new DynamicTableEntity("agg1", "test-1", "etag",
                                             new Dictionary<string, EntityProperty>
                                                 {
                                                     {
                                                         "d1",
                                                         new EntityProperty(new byte[0])
                                                     }
                                                 });
            _row = new BatchOperationRow(_entity, false);
        };

        private It should_have_columns = () => _row.Columns.ShouldNotBeEmpty();

        private It should_have_a_size_of_zero = () => _row.Size.ShouldEqual(0);

        private It should_not_be_dirty = () => _row.IsDirty.ShouldBeFalse();

        private It should_have_an_etag = () => _row.ETag.ShouldEqual(_entity.ETag);

        private It should_have_a_stream_name = () => _row.StreamName.ShouldEqual(_entity.PartitionKey);

        private It should_have_a_sequence = () => _row.Sequence.ShouldEqual(1);

        private It should_have_a_sequence_prefix = () => _row.SequencePrefix.ShouldEqual("test");

        private static BatchOperationRow _row;
        private static DynamicTableEntity _entity;
    }
}