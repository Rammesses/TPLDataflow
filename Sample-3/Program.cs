using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using Shared;

namespace Sample_3
{
    public class Program
    {
        const int iterations = 99;
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        public static async Task Main(string[] args)
        {
            // Set up the input Buffer component
            var buffer = new BufferBlock<Winner>();

            // set up the Action components to run in parallel
            var actionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = processorCount
            };


            var joinFlows = new JoinBlock<Winner, Winner>();

            var splitWinners = new ActionBlock<Winner>(
                async winner => {
                    var isFemale = await Winner.IsFemale(winner);
                    if (isFemale)
                    {
                        await joinFlows.Target2.SendAsync(winner);
                    }
                    else
                    {
                        await joinFlows.Target1.SendAsync(winner);
                    }
                },
                actionOptions);

            buffer.LinkTo(splitWinners);
            buffer.Completion.ContinueWith(delegate { splitWinners.Complete(); });

            // If we don't do this it'll hang
            splitWinners.Completion.ContinueWith(delegate { joinFlows.Complete(); });


            var createMatches = new BatchBlock<Tuple<Winner, Winner>>(2);

            joinFlows.LinkTo(createMatches);
            joinFlows.Completion.ContinueWith(delegate { createMatches.Complete(); });

            int matchIndex = 0;
            var outputMatches = new ActionBlock<Tuple<Winner, Winner>[]>(
                pairs =>
                {
                    var pair1 = pairs.First();
                    var pair2 = pairs.Last();

                    Console.WriteLine($" - O({matchIndex++}): {pair1.Item1.Person} and {pair1.Item2.Person} vs {pair2.Item1.Person} and {pair2.Item2.Person}");
                });

            createMatches.LinkTo(outputMatches);
            createMatches.Completion.ContinueWith(delegate { outputMatches.Complete(); });
            // fill the buffer
            var winners = await Generator.Generate(iterations);
            foreach (var winner in winners)
            {
                await buffer.SendAsync<Winner>(winner);
            }

            // flag no more data
            buffer.Complete();

            // wait for the action block to complete
            await buffer.Completion;
            await splitWinners.Completion;
            await joinFlows.Completion;

            Console.WriteLine("M: Processed all input.");

            Console.WriteLine("M: Done");
        }



    }
}
