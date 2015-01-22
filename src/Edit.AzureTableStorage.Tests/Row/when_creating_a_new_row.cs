using System.Linq;
using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Row
{
    public class when_creating_a_new_row
    {
        private Because of = () =>
        {
            _streamName = "agg1";
            _sequence = 1;
            _sequencePrefix = "test";
            _row = new BatchOperationRow(_streamName, _sequencePrefix, _sequence, false);
        };

        private It should_have_one_column = () => _row.Columns.Count().ShouldEqual(1);

        private It should_have_a_size_of_zero = () => _row.Size.ShouldEqual(0);

        private It should_be_dirty = () => _row.IsDirty.ShouldBeTrue();

        private It should_not_have_an_etag = () => _row.ETag.ShouldBeNull();

        private It should_have_a_stream_name = () => _row.StreamName.ShouldEqual(_streamName);

        private It should_have_a_sequence = () => _row.Sequence.ShouldEqual(_sequence);

        private It should_have_a_sequence_prefix = () => _row.SequencePrefix.ShouldEqual(_sequencePrefix);

        private static BatchOperationRow _row;
        private static string _streamName;
        private static long _sequence;
        private static string _sequencePrefix;
    }
}