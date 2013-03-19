namespace Pretzel.Tests.Templating.Jekyll
{
    public static class StringTestExtensions
    {
        public static string RemoveWhiteSpace(this string s)
        {
            return s.Replace("\r\n", "").Replace("\n", "");
        }
    }
}