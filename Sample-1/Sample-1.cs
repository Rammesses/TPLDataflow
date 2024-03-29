﻿using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Shared;

namespace Sample_1
{
    class Program
    {
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        const int numberOfPlayers = 5;

        static async Task Main(string[] args)
        {
            // Set up the dataflow (correctly)
            // ===============================

            Console.WriteLine("M: Setting up dataflow...");

            // Set up the input Buffer component
            var inputBufferBlock = new BufferBlock<Player>();

            // set up the Action component
            var practiceActionBlock = new ActionBlock<Player>(
                async player =>
                {
                    Console.WriteLine($" --- A({player.Index}): {player.Name} is practicing...");
                    await Task.Delay(player.SlothFactor);
                    Console.WriteLine($" --- A({player.Index}): {player.Name} was practicing for {player.SlothFactor.TotalSeconds}s");
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = processorCount
                });


            // Link the blocks together
            // ========================

            // link the Buffer output to the Action input
            inputBufferBlock.LinkTo(practiceActionBlock);

            // let the Buffer tell the Action when it's done.
            inputBufferBlock.Completion.ContinueWith(delegate { practiceActionBlock.Complete(); });


            // Fill the buffer Asynchronously (C#8 IAsyncEnumerable!)
            // ======================================================

            // fill the buffer
            Console.WriteLine("M: Filling the inputBufferBlock...");

            var enumerator = Generator.GenerateAsync().GetAsyncEnumerator();
            for (var i = 0; i < numberOfPlayers; i++)
            {
                await enumerator.MoveNextAsync();
                var player = enumerator.Current;
                await inputBufferBlock.SendAsync<Player>(player);
            }

            // Signal no more data
            inputBufferBlock.Complete();

            Console.WriteLine("M: Buffer is full");


            // Wait for completion
            // ===================

            // wait for the buffer to empty
            await inputBufferBlock.Completion;

            Console.WriteLine("M: Buffer is empty.");

            // wait for the action block to complete
            await practiceActionBlock.Completion;

            Console.WriteLine("M: Processed all input.");
        }
    }
}
