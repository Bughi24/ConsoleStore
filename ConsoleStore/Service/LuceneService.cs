using Lucene.Net.Util;
using ConsoleStore.Models;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using UglyToad.PdfPig;
using System.IO;

namespace ConsoleStore.Service
{
    public class LuceneService
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly string _indexPath = Path.Combine(Environment.CurrentDirectory, "LuceneIndex");

        private readonly string _pdfFolderPath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "pdf");

        public void BuildIndex(List<Product> products)
        {
            if (!System.IO.Directory.Exists(_indexPath)) System.IO.Directory.CreateDirectory(_indexPath);

            using var dir = FSDirectory.Open(_indexPath);
            var analyser = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyser);
            using var writer = new IndexWriter(dir, indexConfig);

            writer.DeleteAll();

            foreach (var product in products)
            {
                
                string pdfText = ExtractTextFromPDF(product.ProductId);

                var doc = new Document
                {
                    new StringField("Id", product.ProductId.ToString(), Field.Store.YES),
                    new TextField("Name", product.Name ?? "", Field.Store.YES),
                    new TextField("PdfContent", pdfText, Field.Store.NO)
                };
                writer.AddDocument(doc);
            }
            writer.Commit();
        }

        private string ExtractTextFromPDF(int productId)
        {

            string filePath = Path.Combine(_pdfFolderPath, $"{productId}.pdf");

            try
            {
                using var pdf = PdfDocument.Open(filePath);
                string text = "";
                foreach (var page in pdf.GetPages())
                {
                    text += page.Text + " ";
                }
                System.Diagnostics.Debug.WriteLine($"---> [LUCENE] Succes! Am citit {text.Length} caractere din {productId}.pdf");
                return text;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"---> [LUCENE] Eroare la citirea {productId}.pdf: {ex.Message}");
                return "";
            }
        }
            
        public Dictionary<int, float> SearchWithScore(string searchTerm)
        {
            var results = new Dictionary<int, float>();
            if (string.IsNullOrWhiteSpace(searchTerm)) return results;

            using var dir = FSDirectory.Open(_indexPath);
            if (!DirectoryReader.IndexExists(dir)) return results;

            using var reader = DirectoryReader.Open(dir);
            var searcher = new IndexSearcher(reader);
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            string[] fields = { "Name", "PdfContent" };
            var queryParser = new MultiFieldQueryParser(AppLuceneVersion, fields, analyzer);

            var query = queryParser.Parse(searchTerm.Trim() + "*");

            var hits = searcher.Search(query, 20).ScoreDocs;

            foreach (var hit in hits)
            {
                var doc = searcher.Doc(hit.Doc);
                int id = int.Parse(doc.Get("Id"));

                results[id] = hit.Score;
            }

            return results;
        }
    }
}