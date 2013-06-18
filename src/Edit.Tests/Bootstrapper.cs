using Edit.AzureTableStorage;
using Edit.JsonNet;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Edit.Tests
{
    public class Bootstrapper
    {
        public static IStreamStore WireupEventStore()
        {
            var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            AzureTableStorageAppendOnlyStore.IsStorageEmulator = true;
            return AzureTableStorageAppendOnlyStore.CreateAsync(
                cloudStorageAccount, 
                "assumptions", 
                new JsonNetSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects })).Result;
            //var tableStore = AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, "assumptions").Result;
            //return new StreamStore(tableStore, new JsonNetSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects }));
        }
    }
}
