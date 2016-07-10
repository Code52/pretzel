using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pretzel.Logic.Liquid
{
    public static class DefaultFilter
    {
        public static string Default(string input, string @defaultValue)
        {
            return !string.IsNullOrWhiteSpace(input) ? input : defaultValue;
        }
    }
}