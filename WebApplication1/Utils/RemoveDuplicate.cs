using System.Text;

namespace WebApplication1.Utils
{
    public static class RemoveDuplicate
    {
        public static string RemoveDup(string str)
        {

            //HashSet Removes Duplicate Characters
            HashSet<char> charSet = new HashSet<char>();
            //Use String Builder to append new string without dupliactes
            StringBuilder result = new StringBuilder();
            foreach (char c in str)
            {
                if(charSet.Add(c))
                {
                    result.Append(c);
                }
            }
            return result.ToString();

        }
    }
}
