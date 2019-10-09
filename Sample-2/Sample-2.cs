using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using Shared;

namespace Sample_2
{
    class Program
    {
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        const int numberOfPlayers = 25;

        // gets a list of winners
        // parse their Wikipedia page data
        // summaraise the winners by year retired
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Finding years of birth for {numberOfPlayers} players...\n");

            // Set up the dataflow
            // ===================

            var inputBufferBlock = new BufferBlock<Player>();

            var actionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = processorCount
            };

            var getPlayerYearOfBirthTransformBlock = new TransformBlock<Player, int>(
                async (player) => {
                    Console.WriteLine($" - Getting birthday for {player.Name}...");
                    var year = await Player.GetYearOfBirth(player);

                    if (year > 0)
                    {
                        Console.WriteLine($" - {player.Name} was born in {year}.");
                    }
                    else
                    {
                        Console.WriteLine($" - {player.Name} has no birthday on Wikipedia!");
                    }

                    return year;
                },

                actionOptions
            );

            var summariseCustomBlock = new SummarizerBlock();

            var outputYearCountsActionBlock = new ActionBlock<Tuple<int, int>>(            
                input =>
                {
                    var year = input.Item1;
                    Console.WriteLine($"{input.Item2} players were born in {((year < 0) ? "Unknown" : year.ToString())}");

                },
            actionOptions);

            // Link the blocks together
            // ========================

            inputBufferBlock.LinkTo(getPlayerYearOfBirthTransformBlock);
            inputBufferBlock.Completion.ContinueWith(delegate { getPlayerYearOfBirthTransformBlock.Complete(); });

            getPlayerYearOfBirthTransformBlock.LinkTo(summariseCustomBlock);
            getPlayerYearOfBirthTransformBlock.Completion.ContinueWith(delegate { summariseCustomBlock.Complete(); });

            summariseCustomBlock.LinkTo(outputYearCountsActionBlock);
            summariseCustomBlock.Completion.ContinueWith(delegate { outputYearCountsActionBlock.Complete(); });



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
            await outputYearCountsActionBlock.Completion;

            Console.WriteLine("\nDone");
        }   
    }
}
