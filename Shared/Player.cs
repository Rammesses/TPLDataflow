using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Shared
{
    public class Player
    {
        private static Random random = new Random(DateTime.Now.Millisecond);

        public Player(int index, string person)
        {
            this.Index = index;
            this.Name = person;
            this.SlothFactor = TimeSpan.FromMilliseconds(random.Next(5000));
        }

        public int Index { get; }
        public string Name { get; }
        public TimeSpan SlothFactor { get; }

        private static Regex isFemaleMatcher = new Regex(@"women's|Women's");

        public static async Task<bool> IsFemale(Player player)
        {
            var encodedName = HttpUtility.UrlEncode(player.Name);
            var url = $"https://en.wikipedia.org/w/api.php?action=parse&page={encodedName}&prop=wikitext&section=0&format=json";

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);

                var matches = isFemaleMatcher.Matches(response);
                return matches.Any();
            }
        }

        private static Regex dateOfBirthMatcher = new Regex(@"birth_date = {{[A-Za-z\s\|\=]*(\d{4})\|(\d+)\|(\d+)}}");

        public static async Task<int> GetYearOfBirth(Player player)
        {
            var encodedName = HttpUtility.UrlEncode(player.Name);
            var url = $"https://en.wikipedia.org/w/api.php?action=parse&page={encodedName}&prop=wikitext&section=0&format=json";

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);

                var matches = dateOfBirthMatcher.Matches(response);
                if (matches.Any())
                {
                    var match = matches.First();
                    var yearString = match.Groups[1].Value;
                    var year = int.Parse(yearString);
                    return year;
                }
            }

            return -1;
        }
    }
}