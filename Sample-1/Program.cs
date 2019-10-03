using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sample_1
{
    class Program
    {
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        static async Task Main(string[] args)
        {
            // generate some data
            var inputData = await Generator.Generate(10);

            var bufferOptions = new DataflowBlockOptions
            {
            };

            var buffer = new BufferBlock<Input>(bufferOptions);
            
            var actionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = processorCount
            };

            var action = new ActionBlock<Input>(
                async input =>
                    {
                        await Task.Delay(input.Delay);
                        Console.WriteLine($"A: Hello {input.Person} ({input.Index})");
                    },
                actionOptions);

            buffer.LinkTo(action);
            buffer.Completion.ContinueWith(delegate { action.Complete(); });

            // fill the buffer
            Console.WriteLine("M: Filling the buffer...");
            foreach (var input in inputData)
            {
                buffer.Post(input);
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(500)));
            }

            // flag no more data
            buffer.Complete();
            Console.WriteLine("M: Filled the buffer.");

            // wait for the buffer to empty
            await buffer.Completion;
            Console.WriteLine("M: Buffer is empty.");

            // wait for the action block to complete
            await action.Completion;

            Console.WriteLine("M: All done.");
        }
    }
}
