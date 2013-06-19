using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edit.AzureTableStorage;

namespace Edit.PerformanceTests
{
    public class MultipleRowReadTest
    {
        private const int NoAggregates = 100000;
        private static readonly byte[] PayLoad = new byte[10*1024]; // 10KB payload
        private const int MaxTasks = 200;
        int MaxChunksPerTransaction
        {
            get
            {
                if (AzureTableStorageAppendOnlyStore.IsStorageEmulator)
                {
                    return 10;
                }
                return 50;
            }
        }

        private List<Chunk> AddChunks(List<Chunk> chunks, int noChunksToAdd)
        {
            for (int i = 0; i < noChunksToAdd; i++)
            {
                chunks.Add(new Chunk
                {
                    Instance = new CreatedCustomer
                    {
                        Id = new Guid(),
                        Name = "ChunkNo" + i.ToString(),
                        PayLoad = PayLoad
                    }
                });
            }
            return chunks;
        }

        private int _noTasksComplete = 0;
        private void ReportTaskComplete()
        {
            _noTasksComplete++;
            if (_noTasksComplete%500 == 0)
            {
                Console.WriteLine(_noTasksComplete + " aggregates written");
            }
        }

        private async Task<IStoredDataVersion> WriteRead(IStreamStore eventStore, String partitionKey, List<Chunk> chunks, int noChunksToWrite, IStoredDataVersion version)
        {
            await eventStore.WriteAsync(partitionKey, chunks.Take(noChunksToWrite), version);
            return eventStore.ReadAsync(partitionKey).Result.Version;
        }

        private async Task WriteAggregate(IStreamStore eventStore, String partitionKey, List<Chunk> chunks)
        {
            if (chunks.Count > MaxChunksPerTransaction)
            {
                IStoredDataVersion version = null;
                int chunksToWrite = 0;
                int chunkCount = chunks.Count;
                while (true)
                {
                    chunksToWrite += MaxChunksPerTransaction;
                    if (chunksToWrite > chunkCount)
                    {
                        chunksToWrite = chunkCount;
                    }
                    version = await WriteRead(eventStore, partitionKey, chunks, chunksToWrite, version);
                    if (chunksToWrite == chunkCount)
                    {
                        break;
                    }
                }
            }
            else
            {
                await eventStore.WriteAsync(partitionKey, chunks, null);
            }
            ReportTaskComplete();
        }

        private async Task AssureManyTableRows(IStreamStore eventStore)
        {
            var chunkSet = await eventStore.ReadAsync(NoAggregates.ToString());
            if (chunkSet == null)
            {
                var stopWatch = new Stopwatch();

                Console.WriteLine("Running {0} insertions", NoAggregates);
                stopWatch.Start();


                List<Chunk> chunks = new List<Chunk>();
                AddChunks(chunks, 1);
                int cnt = 0;

                ConcurrentQueue<Task> previousTasks = null, tasks = null;
                for (int j = 0; j < NoAggregates/MaxTasks; j++)
                {
                    previousTasks = tasks;
                    tasks = new ConcurrentQueue<Task>();

                    for (int i = 1; i <= MaxTasks; i++)
                    {
                        cnt++;
                        if (i%(NoAggregates/100) == 0)
                        {
                            AddChunks(chunks, 5);
                        }
                        var task = WriteAggregate(eventStore, cnt.ToString(), chunks);
                        tasks.Enqueue(task);
                    }

                    //Console.WriteLine("Tasks created");
                    Task.WhenAll(tasks);
                    if (previousTasks != null)
                    {
                        await Task.WhenAll(previousTasks);
                    }
                    //Console.WriteLine("Tasks completed");
                }
                await Task.WhenAll(tasks);
                await Task.WhenAll(previousTasks);
                stopWatch.Stop();
            }
            else
            {
                Console.WriteLine("Data already in database. Proceed to read tests");
            }
        }

        private int noRowsRead;
        private long payloadRead;
        private object sumLock = new object();
        private async Task Read(IStreamStore eventStore, String partitionKey)
        {
            var chunkSet = await eventStore.ReadAsync(partitionKey);
            /*
            var version = chunkSet.Version as AzureTableStorageEntryDataVersion;
            lock (sumLock)
            {
                noRowsRead += version.LastRowKey + 1;
            }
             */
        }

        private async Task ReadAll(IStreamStore eventStore)
        {
            Console.Write("Reading");
            noRowsRead = 0;
            payloadRead = 0;
            Stopwatch stopWatch = new Stopwatch();
            var currTasks = new ConcurrentQueue<Task>();
            var previousTasks = new ConcurrentQueue<Task>();
            stopWatch.Start();
            int cnt = 0;

            for (int j = 0; j < NoAggregates/MaxTasks; j++)
            {
                var tmpTasks = previousTasks;
                previousTasks = currTasks;
                currTasks = tmpTasks;
                for (int i = 1; i <= MaxTasks; i++)
                {
                    cnt++;
                    currTasks.Enqueue(Read(eventStore, cnt.ToString()));
                }
                Task.WhenAll(currTasks);
                await Task.WhenAll(previousTasks);
                if (cnt%100 == 0)
                {
                    Console.Write(".");
                }
            }
            await Task.WhenAll(currTasks);
            await Task.WhenAll(previousTasks);
            stopWatch.Stop();
            Console.WriteLine(" " + noRowsRead + " rows read");
            Console.WriteLine(stopWatch.ElapsedMilliseconds / 1000 + "s");            
        }

        public async Task Run()
        {
            var eventStore = await Bootstrapper.WireupEventStoreAsync();
            await AssureManyTableRows(eventStore);

            Console.WriteLine("Starting multi read test");
            await ReadAll(eventStore);

            Console.WriteLine("Starting 2 read test");
            eventStore = await Bootstrapper.WireupEventStoreAsync(new RetrieveThenSingleQueryEntitiesReader());
            await ReadAll(eventStore);

            Console.WriteLine("Starting single read test");
            eventStore = await Bootstrapper.WireupEventStoreAsync(new SingleQueryEntitiesReader());
            await ReadAll(eventStore);

        }
    }
}
