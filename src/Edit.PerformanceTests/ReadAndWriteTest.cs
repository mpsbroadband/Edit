using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Edit.AzureTableStorage;
using Edit.JsonNet;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Edit.PerformanceTests
{
    public class ReadAndWriteTest
    {
        private static readonly List<Guid> _ids = new List<Guid>();

        const int NumberOfInsertions = 1000;

        public async Task Run()
        {
            var eventStore = await Bootstrapper.WireupEventStoreAsync();
            var stopWatch = new Stopwatch();

            Console.WriteLine("Running {0} insertions", NumberOfInsertions);
            stopWatch.Start();

            var tasks = new ConcurrentQueue<Task>();

            for (var i = 0; i < NumberOfInsertions; i++)
            {
                var e = new CreatedCustomer(Guid.NewGuid(), "Edit");

                var task = eventStore.WriteAsync(e.Id.ToString(), new List<Chunk>()
                    {
                        new Chunk() {Instance = e}
                    }, null);
                tasks.Enqueue(task);

                _ids.Add(e.Id);
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();

            Console.WriteLine("Time elapsed {0} seconds", stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine("{0} writes per second in average", NumberOfInsertions / stopWatch.Elapsed.TotalSeconds);

            stopWatch.Reset();
            tasks = new ConcurrentQueue<Task>();

            Console.WriteLine("Running {0} reads", NumberOfInsertions);
            stopWatch.Start();

            foreach (var id in _ids)
            {
                var task = eventStore.ReadAsync(id.ToString());
                tasks.Enqueue(task);
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();

            Console.WriteLine("Time elapsed {0} seconds", stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine("{0} reads per second in average", NumberOfInsertions / stopWatch.Elapsed.TotalSeconds);
        }
    }

}
