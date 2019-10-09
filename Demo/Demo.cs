using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Shared;

namespace Demo
{
    public static class Program
    {
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        const int numberOfPlayers = 5;

        public static async Task Main(string[] args)
        {
            // Set up the dataflow
            // ===================


            // Link the blocks together
            // ========================


            // Fill the buffer Asynchronously (C#8 IAsyncEnumerable!)
            // ======================================================


            // Wait for completion
            // ===================
        }
    }
}
