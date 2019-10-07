using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared
{
    public class Generator
    {
        private static string[] WimbledonWinners =
        {
            "Rod Laver",
            "John Newcombe",
            "Stan Smith",
            "Jan Kodeš",
            "Jimmy Connors",
            "Arthur Ashe",
            "Björn Borg",
            "John McEnroe",
            "Jimmy Connors",
            "Boris Becker",
            "Pat Cash",
            "Stefan Edberg",
            "Michael Stich",
            "Andre Agassi",
            "Pete Sampras",
            "Richard Krajicek",
            "Goran Ivanišević",
            "Lleyton Hewitt",
            "Roger Federer",
            "Rafael Nadal",
            "Novak Djokovic",
            "Andy Murray",
            "Billie Jean King",
            "Ann Jones",
            "Margaret Court",
            "Evonne Goolagong",
            "Chris Evert",
            "Virginia Wade",
            "Martina Navratilova",
            "Evonne Goolagong Cawley",
            "Martina Navratilova",
            "Steffi Graf",
            "Conchita Martínez",
            "Martina Hingis",
            "Jana Novotná",
            "Lindsay Davenport",
            "Venus Williams",
            "Serena Williams",
            "Maria Sharapova",
            "Amélie Mauresmo",
            "Petra Kvitová",
            "Marion Bartoli",
            "Garbiñe Muguruza",
            "Angelique Kerber",
            "Simona Halep"
        };

        private static Random random = new Random(DateTime.Now.Millisecond);

        public static Task<IEnumerable<Winner>> Generate(int v)
        {
            var result = new List<Winner>();

            for (var i = 0; i < v; i++)
            {
                var winnerIndex = random.Next(WimbledonWinners.Length);
                var winner = WimbledonWinners[winnerIndex];
                result.Add(new Winner(i, winner));
            }

            return Task.FromResult((IEnumerable<Winner>)result);
        }

        public static async IAsyncEnumerable<Winner> GenerateAsync()
        {
            var index = 0;

            while (true)
            {
                var winnerIndex = random.Next(WimbledonWinners.Length);
                var winner = WimbledonWinners[winnerIndex];

                Console.WriteLine($" - G({index}): {winner} enters the restaurant...");
                await Task.Delay(random.Next(2500));
                Console.WriteLine($" - G({index}): {winner} is sat down.");

                yield return new Winner(index++, winner);
            }
        }
    }
}