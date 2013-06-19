using System;
using System.Net;

namespace Edit.PerformanceTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting performance tests");

            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 1000;

            var p = new MultipleRowReadTest();
            p.Run().Wait();

            Console.WriteLine("Finished performance tests");
            Console.Read();
        }

    }
}
