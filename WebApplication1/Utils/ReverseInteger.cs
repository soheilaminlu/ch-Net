using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;

namespace WebApplication1.Utils
{
    public static class ReverseNumber
    {
        // number must be usigned
        public static uint ReverseInteger(uint number)
        {
            uint reversed = 0;
            
            while (number > 0)
            {
                reversed = reversed * 10 + number % 10;
                number /= 10;
            }
            return reversed;

        }
    }
}
