using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared
{
    public class Generator
    {
        private static string[] Wimbledonplayers =
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

        public static async Task<IEnumerable<Player>> Generate(int numberOfPlayers, bool waitForPlayerToBeReady = true)
        {
            var result = new List<Player>();

            for (var index = 0; index < numberOfPlayers; index++)
            {
                var playerIndex = random.Next(Wimbledonplayers.Length);
                var player = new Player(index, Wimbledonplayers[playerIndex]);

                if (waitForPlayerToBeReady)
                {
                    Console.WriteLine($" - G({index}): {player.Name} enters the court...");
                    await Task.Delay(player.SlothFactor);
                    Console.WriteLine($" - G({index}): {player.Name} is ready.");
                }

                result.Add(player);
            }

            return (IEnumerable<Player>)result;
        }

        public static async IAsyncEnumerable<Player> GenerateAsync(bool waitForPlayerToBeReady = true)
        {
            var index = 0;

            while (true)
            {
                var playerIndex = random.Next(Wimbledonplayers.Length);
                var player = new Player(index++, Wimbledonplayers[playerIndex]);

                if (waitForPlayerToBeReady)
                {
                    Console.WriteLine($" - G({index}): {player.Name} enters the court...");
                    await Task.Delay(player.SlothFactor);
                    Console.WriteLine($" - G({index}): {player.Name} is ready.");
                }

                yield return player;
            }
        }
    }
}