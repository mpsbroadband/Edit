using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edit.AzureTableStorage;

namespace Edit.PerformanceTests
{
    public class WritingAndReadingLargeAggregatesTest : ITest
    {
        private String CreateLargeString(int noBytes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < noBytes; i++)
            {
                sb.Append((byte) 1);
            }
            return sb.ToString();
        }

        private List<Chunk> CreateLargeAggregate(int noMB)
        {
            const int noBytesString = 100*1024; //100 KB
            var chunks = new List<Chunk>();
            String largeString = CreateLargeString(noBytesString);           

            for (int i = 0; i < 10*noMB; i++)
            {
                chunks.Add(new Chunk { Instance = new CreatedCustomer { Name = largeString + i }});
            }

            return chunks;
        }

        public async Task Run()
        {
            var eventStore = await Bootstrapper.WireupEventStoreAsync();

            Console.WriteLine("Event store set up");

            var aggregate = CreateLargeAggregate(4);
            //String id = Guid.NewGuid().ToString();
            String id = new Guid().ToString();

            IStoredDataVersion version = null;
            ChunkSet chunkSet = null;

            Console.Write("Writing");

            for (int i = 1; i <= aggregate.Count; i++)
            {
                await eventStore.WriteAsync(id, aggregate.Take(i), version);
                chunkSet = eventStore.ReadAsync(id).Result;
                version = chunkSet.Version;
                Console.Write(".");
            }
            Console.WriteLine();
            int totalStringLength = 0;
            foreach (var chunk in chunkSet.Chunks)
            {
                totalStringLength += (chunk.Instance as CreatedCustomer).Name.Length;
            }
            Console.WriteLine("No chunks read: {0} chunks. Total string length: {1}", chunkSet.Chunks.Count(), totalStringLength);
        }
    }
}
