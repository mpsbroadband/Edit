using System;

namespace Edit.AzureTableStorage.IntegrationTests
{
    public interface IEvent
    {
        Guid Id { get; }
    }
}