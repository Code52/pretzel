using System;

namespace Pretzel.Logic.Liquid
{
    public class CgiEscapeFilter
    {
        public static string cgi_escape(string input)
        {
            return Uri.EscapeDataString(input);
        }
    }
}