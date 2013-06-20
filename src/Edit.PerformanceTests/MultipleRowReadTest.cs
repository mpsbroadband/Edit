using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Edit.AzureTableStorage;
using Edit.JsonNet;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Edit.PerformanceTests
{
    public class MultipleRowReadTest : ITest
    {
        private const int NoAggregates = 95000;
        private static readonly byte[] PayLoad = new byte[10*1024]; // 10KB payload
        private const int MaxTasks = 20;
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
            var newList = new List<Chunk>();
            newList.AddRange(chunks);
            for (int i = 0; i < noChunksToAdd; i++)
            {
                newList.Add(new Chunk
                {
                    Instance = new CreatedCustomer
                    {
                        Id = new Guid(),
                        Name = "ChunkNo" + i.ToString(),
                        PayLoad = PayLoad
                    }
                });
            }
            return newList;
        }

        private int _noTasksComplete;
        private Stopwatch taskCompleteWatch = new Stopwatch();
        private void ReportTaskComplete(List<Chunk> chunks)
        {
            _noTasksComplete++;
            if (_noTasksComplete%100 == 0)
            {
                taskCompleteWatch.Stop();
                Console.WriteLine("{0} aggregates written. {1} chunks in last written aggregate. Time: {2}s", _noTasksComplete, chunks.Count, taskCompleteWatch.ElapsedMilliseconds / 1000);
                taskCompleteWatch.Reset();
                taskCompleteWatch.Start();
            }
        }

        private async Task<IStoredDataVersion> WriteRead(IStreamStore eventStore, String partitionKey, List<Chunk> chunks, int noChunksToWrite, IStoredDataVersion version)
        {
            await eventStore.WriteAsync(partitionKey, chunks.Take(noChunksToWrite), version);
            return eventStore.ReadAsync(partitionKey).Result.Version;
        }

        private void PrintStorageEx(String partitionKey, StorageException storageException)
        {
            Console.WriteLine("Key: " + partitionKey + "ERROR: " + storageException.RequestInformation.ExtendedErrorInformation.ErrorCode + " " + storageException.RequestInformation.ExtendedErrorInformation.ErrorMessage);
        }

        private async Task WriteAggregate(IStreamStore eventStore, String partitionKey, List<Chunk> chunks)
        {
            try
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
                ReportTaskComplete(chunks);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is StorageException)
                {
                    PrintStorageEx(partitionKey, ex.InnerException as StorageException);
                    throw;
                }
            }
            catch (StorageException ex)
            {
                PrintStorageEx(partitionKey, ex);
                throw;
            }
        }

        private async Task<int> AttemptToResume(IStreamStore eventStore)
        {
            int currTestNo = NoAggregates;
            do
            {
                var chunkSet = await eventStore.ReadAsync(currTestNo.ToString());
                if (chunkSet != null)
                {
                    int foundNo = currTestNo;
                    for (int i = foundNo; i < foundNo + 1000; i = i + 100)
                    {
                        chunkSet = await eventStore.ReadAsync(i.ToString());
                        if (chunkSet == null)
                        {
                            break;
                        }
                        currTestNo = i;
                    }
                    break;
                }
                currTestNo -= 1000;
            } while (currTestNo > 0);
            return currTestNo;
        }

        private async Task AssureManyTableRows(IStreamStore eventStore)
        {
            Console.WriteLine("Running {0} insertions", NoAggregates);

            _noTasksComplete = await AttemptToResume(eventStore);
            int cnt = 0;
            List<Chunk> chunks = new List<Chunk>();
            if (_noTasksComplete >= NoAggregates)
            {
                Console.WriteLine("Data already in database. Proceed to read tests");
                return;
            }
            if (_noTasksComplete > 0)
            {
                cnt = _noTasksComplete;
                chunks = AddChunks(chunks, cnt/(NoAggregates/20)*5);
                Console.WriteLine("Resuming writes from #{0} with {1} chunks", cnt, chunks.Count);
            }
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            chunks = AddChunks(chunks, 1);
            taskCompleteWatch.Start();
            ConcurrentQueue<Task> previousTasks = null, tasks = null;
            for (int j = 0; j < NoAggregates/MaxTasks; j++)
            {
                previousTasks = tasks;
                tasks = new ConcurrentQueue<Task>();

                for (int i = 1; i <= MaxTasks; i++)
                {
                    cnt++;
                    if (cnt%(NoAggregates/20) == 0)
                    {
                        chunks = AddChunks(chunks, 5);
                    }
                    var task = WriteAggregate(eventStore, cnt.ToString(), chunks);
                    tasks.Enqueue(task);
                    if (cnt >= NoAggregates)
                    {
                        break;
                    }
                }

                //Console.WriteLine("Tasks created");
                Task.WhenAll(tasks);
                if (previousTasks != null)
                {
                    await Task.WhenAll(previousTasks);
                }
                if (cnt >= NoAggregates)
                {
                    break;
                }
                Console.Write(".");
                if (cnt%500 == 0)
                {
                    Console.Write(cnt);
                }
                //Console.WriteLine("Tasks completed");
            }
            await Task.WhenAll(tasks);
            await Task.WhenAll(previousTasks);
            stopWatch.Stop();
        }

        private async Task ReadAll(IStreamStore eventStore)
        {
            Console.Write("Reading");
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
                    currTasks.Enqueue(eventStore.ReadAsync(cnt.ToString()));
                }
                Task.WhenAll(currTasks);
                await Task.WhenAll(previousTasks);
                if (cnt%100 == 0)
                {
                    Console.Write(".");
                }
                if (cnt%1000 == 0)
                {
                    Console.Write(cnt);
                }
            }
            await Task.WhenAll(currTasks);
            await Task.WhenAll(previousTasks);
            stopWatch.Stop();
            Console.WriteLine(stopWatch.ElapsedMilliseconds / 1000 + "s");            
        }

        public async Task Run()
        {
            ISerializer serializer =
                new JsonNetSerializer(new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects});
            var framer = new DummyReadFramer(serializer);
            var eventStore = await Bootstrapper.WireupEventStoreAsync(new MultipleRetrieveEntitiesReader(), framer);
            await AssureManyTableRows(eventStore);

            Console.WriteLine("Starting multi read test");
            await ReadAll(eventStore);

            Console.WriteLine("Starting 2 read test");
            eventStore = await Bootstrapper.WireupEventStoreAsync(new RetrieveThenSingleQueryEntitiesReader(), framer);
            await ReadAll(eventStore);

            Console.WriteLine("Starting single read test");
            eventStore = await Bootstrapper.WireupEventStoreAsync(new SingleQueryEntitiesReader(), framer);
            await ReadAll(eventStore);

        }
    }
}
