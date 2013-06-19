using System.Configuration;
using System.Threading.Tasks;
using Edit.AzureTableStorage;
using Edit.JsonNet;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Edit.PerformanceTests
{
    public class Bootstrapper
    {
        public static Task<IStreamStore> WireupEventStoreAsync()
        {
            return WireupEventStoreAsync(new MultipleRetrieveEntitiesReader());
        }

        public static async Task<IStreamStore> WireupEventStoreAsync(IEntitiesReader entitiesReader)
        {
            //var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageAccountConnectionString"]);
            //AzureTableStorageAppendOnlyStore.IsStorageEmulator = true;
            return await AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, "performancetests", new JsonNetSerializer(new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects }));
        }

    }
}
