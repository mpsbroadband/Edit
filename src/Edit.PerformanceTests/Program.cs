using System;
using System.Net;
using Microsoft.WindowsAzure.Storage;

namespace Edit.PerformanceTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting performance tests");

            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 1000;

            try
            {
                ITest p = new MultipleRowReadTest();
                p.Run().Wait();
            }
            catch (AggregateException ex)
            {
                var storageEx = ex.InnerException as StorageException;
                if (storageEx != null)
                {
                    Console.WriteLine("ERROR: " + storageEx.RequestInformation.ExtendedErrorInformation.ErrorCode + " " + storageEx.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                }
                throw;
            }
            Console.WriteLine("Finished performance tests");
            Console.Read();
        }

    }
}
