namespace Pretzel.Logic.Extensibility.Extensions
{
    internal class CommonMarkEngine : ILightweightMarkupEngine
    {
        public string Convert(string source)
        {
            return CommonMark.CommonMarkConverter.Convert(source);
        }
    }
}
