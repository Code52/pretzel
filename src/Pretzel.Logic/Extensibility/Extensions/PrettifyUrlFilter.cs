using System;

namespace Pretzel.Logic.Extensibility.Extensions
{
    public class PrettifyUrlFilter : IFilter
    {
        public string Name
        {
            get { return "PrettifyUrl"; }
        }

        public static string PrettifyUrl(string input)
        {
            return input.Replace("index.html", String.Empty);
        }
    }
}
