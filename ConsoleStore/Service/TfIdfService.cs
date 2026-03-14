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

        public Dictionary<string, double> Vectorize(string text)
        {
            var words = text.ToLower()
                            .Split(" ", StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, double> tf = new();

            foreach (var word in words) {
                if (!tf.ContainsKey(word))
                    tf[word] = 0;

                tf[word]++;
            }

            int totalWords = words.Length;

            foreach(var key in tf.Keys.ToList())
            {
                tf[key] = tf[key] / totalWords;
            }

            return tf;
        }
        public double CosineSimilarity(Dictionary<string, double> v1, Dictionary<string, double> v2)
        {
            double dot = 0;
            double normA = 0;
            double normB = 0;

            foreach (var key in v1.Keys)
            {
                if (v2.ContainsKey(key))
                {
                    dot += v1[key] * v2[key];
                }

                normA += Math.Pow(v1[key], 2);
            }

            foreach (var val in v2.Values)
            {
                normB += Math.Pow(val, 2);
            }

            if (normA == 0 || normB == 0)
                return 0;

            return dot/(Math.Sqrt(normA) * Math.Sqrt(normB));   
        }
        
        public List<Product> GetSimilarProducts(Product product, List<Product> products)
        {
            var vectors = products.ToDictionary(
                p => p,
                p => Vectorize(p.Name));

            var targetVector = vectors[product];

            var similar = products.Where(p => p.ProductId != product.ProductId)
                                  .Select(p => new
                                  {
                                      Product = p,
                                      Score = CosineSimilarity(targetVector, vectors[p])
                                  })
                                  .OrderByDescending(x => x.Score)
                                  .Take(3)
                                  .Select(x => x.Product)
                                  .ToList();
            return similar;
        }

    }

}
