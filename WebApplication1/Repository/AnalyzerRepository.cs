using System.Text;
using WebApplication1.Interfaces;

namespace WebApplication1.Repository
{
    public class AnalyzerRepository : IAnalyzerRepository
    {
        public uint ReverseInteger(uint number)
        {
            uint reversedNumber = 0;
            while (number > 0)
            {
                uint remainder = number % 10;
                reversedNumber = (reversedNumber * 10) + remainder;
                number /= 10;
            }
            return reversedNumber;
        }

        public string RemoveDuplicates(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var seenCharacters = new HashSet<char>();
            var result = new StringBuilder();

            foreach (var character in input)
            {
                if (seenCharacters.Add(character))
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }
    }
}