using System;
using System.Xml;

namespace Pretzel.Logic.Liquid
{
    public class DateToXmlSchemaFilter
    {
        public static string date_to_xmlschema(DateTime input)
        {
            return XmlConvert.ToString(input, XmlDateTimeSerializationMode.Local);
        }
    }
}
