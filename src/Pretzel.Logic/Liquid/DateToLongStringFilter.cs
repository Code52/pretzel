using System;
using System.Xml;

namespace Pretzel.Logic.Liquid
{
    public class DateToLongStringFilter
    {
        public static string date_to_long_string(DateTime input)
        {
            //return input.ToString("dd MMMM yyyy");
            return XmlConvert.ToString(input, "dd MMMM yyyy");
        }

        public static string date_to_long_string(string input)
        {
            DateTime inputDate;

            if (DateTime.TryParse(input, out inputDate))
            {
                return date_to_long_string(inputDate);
            }

            return "";
        }
    }
}
