using Edit.JsonNet;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Edit.AzureTableStorage.IntegrationTests
{
    public class AssemblyContext : IAssemblyContext
    {
        public static TableStorageStreamStore Store { get; private set; }

        public void OnAssemblyStart()
        {
            Store = new TableStorageStreamStore(
                new TableOperationSerializer(
                    new JsonNetSerializer(new JsonSerializerSettings
                                              {
                                                  TypeNameHandling = TypeNameHandling.All
                                              })),
                CloudStorageAccount.DevelopmentStorageAccount);
        }

        public void OnAssemblyComplete()
        {
        }
    }
}