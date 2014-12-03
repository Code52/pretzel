using System;

namespace Pretzel.Logic.Liquid
{
    public class UriEscapeFilter
    {
        public static string uri_escape(string input)
        {
            return Uri.EscapeUriString(input);
        }
    }
}