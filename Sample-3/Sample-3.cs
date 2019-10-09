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
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        const int numberOfPlayers = 75;

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Finding matches for {numberOfPlayers} players...\n");

            // Set up the dataflow
            // ===================

            var inputBufferBlock = new BufferBlock<Player>();

            // set up the Action components to run in parallel
            var actionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = processorCount
            };

            var createPairsJoinBlock = new JoinBlock<Player, Player>();

            var separateMenFromWomenActionBlock = new ActionBlock<Player>(
                async player => {
                    var isFemale = await Player.IsFemale(player);
                    if (isFemale)
                    {
                        await createPairsJoinBlock.Target2.SendAsync(player);
                    }
                    else
                    {
                        await createPairsJoinBlock.Target1.SendAsync(player);
                    }
                },
                actionOptions);

            var createMatchesBatchBlock = new BatchBlock<Tuple<Player, Player>>(2);

            int matchIndex = 0;
            var outputMatches = new ActionBlock<Tuple<Player, Player>[]>(
                pairs =>
                {
                    var pair1 = pairs.First();
                    var pair2 = pairs.Last();

                    Console.WriteLine($"#{matchIndex++}: {pair1.Item1.Name} / {pair1.Item2.Name} vs {pair2.Item1.Name} / {pair2.Item2.Name}");
                });

            // Link the blocks together
            // ========================

            var linkOptions = new DataflowLinkOptions()
            {
                PropagateCompletion = true
            };

            // a better way to chain completion!
            inputBufferBlock.LinkTo(separateMenFromWomenActionBlock, linkOptions);
            createPairsJoinBlock.LinkTo(createMatchesBatchBlock, linkOptions);
            createMatchesBatchBlock.LinkTo(outputMatches, linkOptions);

            // But we've a "broken chain", so if we don't do this it'll hang
            separateMenFromWomenActionBlock.Completion.ContinueWith(delegate { createPairsJoinBlock.Complete(); });

            // Fill the buffer Asynchronously (C#8 IAsyncEnumerable!)
            // ======================================================
            var enumerator = Generator.GenerateAsync(false).GetAsyncEnumerator();
            for (var i = 0; i < numberOfPlayers; i++)
            {
                await enumerator.MoveNextAsync();
                var player = enumerator.Current;
                await inputBufferBlock.SendAsync<Player>(player);
            }

            // Signal no more data
            inputBufferBlock.Complete();

            // Wait for completion
            // ===================
            await outputMatches.Completion;

            Console.WriteLine("\nDone");
        }



    }
}
