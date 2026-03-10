using ConsoleStore.Models;
using System.Linq;


namespace ConsoleStore.Service
{

    public class TfIdfService
    {
        private static readonly HashSet<string> stopWords = new()
        {
            "the","a","an","and","or","but","if","to","of","in","on",
            "for","with","is","are","was","were","this","that","these",
            "those","it","its","as","at","by"
        };

        public List<Product> Search(string querry, List<Product> products)
        {
            var terms = querry.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                        .Where(t => !stopWords.Contains(t))
                                        .ToList();

            int totalDocs = products.Count;

            Dictionary<Product, double> scores = new();

            foreach (var product in products)
            {
                string description = product.Description.ToLower();
                var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                       .Where(w => !stopWords.Contains(w))
                                       .ToList();

                double score = 0;

                foreach (var term in terms)
                {

                    int termCount = words.Count(w => w == term);
                    double tf = (double)termCount / words.Count;

                    int docsContainingTerm = products.Count(p => p.Description.ToLower().Contains(term));

                    double idf = Math.Log((double)totalDocs / (1 + docsContainingTerm));

                    score += tf * idf;

                    string name = (product.Name ?? "").ToLower();
                    var nameWords = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (nameWords.Contains(term))
                    {
                        score += 5;
                    }
                }

                scores[product] = score;
            }
            return scores.Where(x => x.Value > 0)
                         .OrderByDescending(x => x.Value)
                         .Select(x => x.Key)
                         .ToList();
        }

    }

}
