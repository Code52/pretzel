using System;

namespace Pretzel.Logic.Liquid
{
    public class DateToStringFilter
    {
        public static string date_to_string(DateTime input)
        {
            return input.ToString("dd MMM yyyy");
        }

        public static string date_to_string(string input)
        {
            DateTime inputDate;

            if (DateTime.TryParse(input, out inputDate))
            {
                return date_to_string(inputDate);
            }
            
            return "";
        }
    }
}
