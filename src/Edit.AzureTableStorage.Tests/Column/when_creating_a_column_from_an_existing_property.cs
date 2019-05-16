using AzureApi.Storage.Table;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_creating_a_column_from_an_existing_property
    {
        private Establish context = () =>
        {
            _property = new EntityProperty(new byte[] { 1, 2, 3 });
        };

        private Because of = () =>
        {
            _column = new BatchOperationColumn(_property);
        };

        private It should_set_data_to_property_binary_value = () => _column.Data.ShouldEqual(_property.BinaryValue);
        
        private It should_have_a_size_of_its_data = () => _column.Size.ShouldEqual(_property.BinaryValue.Length);

        private It should_not_be_dirty = () => _column.IsDirty.ShouldBeFalse();

        private static EntityProperty _property;
        private static BatchOperationColumn _column;
    }
}