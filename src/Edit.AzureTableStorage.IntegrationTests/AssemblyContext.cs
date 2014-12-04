using Edit.JsonNet;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Edit.AzureTableStorage.IntegrationTests
{
    public class AssemblyContext : IAssemblyContext
    {
        public static TableStorageStreamStore StreamStore { get; private set; }
        public static BlobStorageSnapshotStore SnapshotStore { get; private set; }

        public void OnAssemblyStart()
        {
            var serializer = new JsonNetSerializer(
                new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

            StreamStore = new TableStorageStreamStore(
                                    new TableOperationSerializer(serializer),
                                    CloudStorageAccount.DevelopmentStorageAccount);
            SnapshotStore = new BlobStorageSnapshotStore(
                                    serializer, 
                                    CloudStorageAccount.DevelopmentStorageAccount);
        }

        public void OnAssemblyComplete()
        {
        }
    }
}