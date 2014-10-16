using Machine.Specifications;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_creating_a_new_column
    {
        private Because of = () =>
        {
            _column = new BatchOperationColumn();
        };

        private It should_not_have_data = () => _column.Data.ShouldBeEmpty();

        private It should_have_a_size_of_zero = () => _column.Size.ShouldEqual(0);

        private It should_not_be_dirty = () => _column.IsDirty.ShouldBeFalse();

        private static BatchOperationColumn _column;
    }
}