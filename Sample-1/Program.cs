using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Shared;

namespace Sample_1
{
    class Program
    {
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        const int iterations = 20;

        static async Task Main(string[] args)
        {
            // Set up the input Buffer component
            var buffer = new BufferBlock<Winner>();

            // set up the Action component
            var actionOptions = new ExecutionDataflowBlockOptions
            {
                 MaxDegreeOfParallelism = processorCount
            };

            var action = new ActionBlock<Winner>(
                async winner =>
                    {
                        Console.WriteLine($" --- A({winner.Index}): {winner.Person} is eating...");
                        await Task.Delay(winner.Delay);
                        Console.WriteLine($" --- A({winner.Index}): {winner.Person} was eating for {winner.Delay.TotalSeconds}s");
                    },
                actionOptions);

            // link the Buffer output to the Action input
            buffer.LinkTo(action);

            // let the Buffer tell the Action when it's done.
            buffer.Completion.ContinueWith(delegate { action.Complete(); });

            // fill the buffer
            Console.WriteLine("M: Filling the buffer...");

            var enumerator = Generator.GenerateAsync().GetAsyncEnumerator();
            for (var i = 0; i < iterations; i++)
            {
                await enumerator.MoveNextAsync();
                var winner = enumerator.Current;
                await buffer.SendAsync<Winner>(winner);
            }

            // flag no more data
            buffer.Complete();

            Console.WriteLine("M: Filled the buffer.");

            // wait for the buffer to empty
            await buffer.Completion;
            Console.WriteLine("M: Buffer is empty.");

            // wait for the action block to complete
            await action.Completion;
            Console.WriteLine("M: Action has processed all input.");
        }
    }
}
