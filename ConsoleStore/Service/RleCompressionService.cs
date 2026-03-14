using System.Text;

namespace ConsoleStore.Service
{
    public class RleCompressionService
    {
        public string Compress(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder result = new StringBuilder();
            int count = 1;

            for (int i = 1; i <= input.Length; i++)
            {

                if (i < input.Length && input[i] == input[i - 1])
                {
                    count++;
                }
                else
                {
                    result.Append(input[i - 1]);
                    result.Append(count);
                    count = 1;
                }
            }

            string compressed = result.ToString();

            return result.ToString();
        }
    }
}