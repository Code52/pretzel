using System;

namespace Pretzel.Logic.Liquid
{
    public class DateToXmlSchemaFilter
    {
        public static string date_to_xmlschema(DateTime input)
        {
            return input.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
