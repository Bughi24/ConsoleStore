using ConsoleStore.Models;

namespace ConsoleStore.Service
{
    public class AutocompleteService
    {
        public int Levenshtein(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1,
                                 d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }

        public List<string> GetSuggestions(string query, List<Product> products)
        {
            query = query.ToLower();

            var suggestions = products
                .Select(p => p.Name)
                .Distinct()
                .Select(name => new
                {
                    Name = name,
                    Distance = Levenshtein(query, name.ToLower())
                })
                .Where(x =>
                    x.Name.ToLower().StartsWith(query)  
                    || x.Distance <= 3                  
                )
                .OrderBy(x => x.Distance)
                .Take(5)
                .Select(x => x.Name)
                .ToList();

            return suggestions;
        }
    }
}