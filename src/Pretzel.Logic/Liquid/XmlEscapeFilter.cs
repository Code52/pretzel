using System.Security;

namespace Pretzel.Logic.Liquid
{
    public static class XmlEscapeFilter
    {
        public static string xml_escape(string input)
        {
            return SecurityElement.Escape(input);
        }
    }
}