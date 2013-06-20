using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Edit.PerformanceTests
{
    public class ReadAndWriteTest : ITest
    {
        private static readonly List<Guid> Ids = new List<Guid>();

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

                var task = eventStore.WriteAsync(e.Id.ToString(), new List<Chunk>
                    {
                        new Chunk {Instance = e}
                    }, null);
                tasks.Enqueue(task);

                Ids.Add(e.Id);
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();

            Console.WriteLine("Time elapsed {0} seconds", stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine("{0} writes per second in average", NumberOfInsertions / stopWatch.Elapsed.TotalSeconds);

            stopWatch.Reset();
            tasks = new ConcurrentQueue<Task>();

            Console.WriteLine("Running {0} reads", NumberOfInsertions);
            stopWatch.Start();

            foreach (var id in Ids)
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
