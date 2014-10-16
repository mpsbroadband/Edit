using Machine.Specifications;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage.Tests.Column
{
    public class when_creating_a_property_from_an_existing_column
    {
        private Establish context = () =>
        {
            _existingProperty = new EntityProperty(new byte[] { 1, 2, 3 });
            _column = new BatchOperationColumn(_existingProperty);
        };

        private Because of = () =>
        {
            _property = _column.ToProperty();
        };

        private It should_have_binary_value_set_to_column_data = () => _property.BinaryValue.ShouldEqual(_column.Data);

        private static BatchOperationColumn _column;
        private static EntityProperty _property;
        private static EntityProperty _existingProperty;
    }
}