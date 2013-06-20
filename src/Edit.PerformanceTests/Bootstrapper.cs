using System;
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
            return WireupEventStoreAsync(new MultipleRetrieveEntitiesReader(), new Framer(new JsonNetSerializer(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects })));
        }

        public static async Task<IStreamStore> WireupEventStoreAsync(IEntitiesReader entitiesReader, IFramer framer)
        {
            //var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            string connectionString = ConfigurationManager.AppSettings["StorageAccountConnectionString"];
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            AzureTableStorageAppendOnlyStore.IsStorageEmulator = connectionString.Equals("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase);
            //AzureTableStorageAppendOnlyStore.IsStorageEmulator = true;
            return await AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, "performancetests", framer, entitiesReader);
        }

    }
}
