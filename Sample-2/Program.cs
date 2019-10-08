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
        const int iterations = 100;
        static int processorCount = Environment.ProcessorCount;
        static Random random = new Random(DateTime.Now.Millisecond);

        // gets a list of winners
        // parse their Wikipedia page data
        // summaraise the winners by year retired
        static async Task Main(string[] args)
        {
            // Set up the input Buffer component
            var buffer = new BufferBlock<Winner>();

            // set up the Action components to run in parallel
            var actionOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = processorCount
            };

            var getWikiData = new TransformBlock<Winner, Tuple<Winner, int>>(
                async (w) => {
                    Console.WriteLine($" - T({w.Index}): Getting birthday for {w.Person}...");
                    var result = await GetDateRetired(w);

                    if (result.Item2 > 0)
                    {
                        Console.WriteLine($" - T({w.Index}): {w.Person} was born in {result.Item2}.");
                    }
                    else
                    {
                        Console.WriteLine($" - T({w.Index}): {w.Person} has no birthday on Wikipedia!");
                    }

                    return result;
                },

                actionOptions
            );

            buffer.LinkTo(getWikiData);
            buffer.Completion.ContinueWith(delegate { getWikiData.Complete(); });

            var summarise = new SummarizerBlock();

            getWikiData.LinkTo(summarise);
            getWikiData.Completion.ContinueWith(delegate { summarise.Complete(); });


            var output = new ActionBlock<Tuple<int, int>>(            
                input =>
                {
                    var year = input.Item1;
                    Console.WriteLine($" - O: {((year < 0) ? "Unknown" : year.ToString())} : {input.Item2}");

                },
            actionOptions);

            summarise.LinkTo(output);
            summarise.Completion.ContinueWith(delegate { output.Complete(); });

            // fill the buffer
            var winners = await Generator.Generate(iterations);
            foreach (var winner in winners)
            {
                buffer.Post<Winner>(winner);
            }

            // flag no more data
            buffer.Complete();

            // wait for the action block to complete
            await buffer.Completion;
            await getWikiData.Completion;
            await output.Completion;
            Console.WriteLine("M: Processed all input.");

            Console.WriteLine("M: Done");
        }

        private static Regex matcher = new Regex(@"birth_date = {{[A-Za-z\s\|\=]*(\d{4})\|(\d+)\|(\d+)}}");

        public static async Task<Tuple<Winner, int>> GetDateRetired(Winner winner)
        {
            var encodedName = HttpUtility.UrlEncode(winner.Person);
            var url = $"https://en.wikipedia.org/w/api.php?action=parse&page={encodedName}&prop=wikitext&section=0&format=json";

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);

                var matches = matcher.Matches(response);
                if (matches.Any())
                {
                    var match = matches.First();
                    var yearString = match.Groups[1].Value;
                    var year = int.Parse(yearString);
                    return new Tuple<Winner, int>(winner, year);
                }
            }

            return new Tuple<Winner, int>(winner, -1);
        }

        
    }
}
