
namespace Pretzel.Logic.Extensibility
{
    /// <summary>
    /// Engine converting lightweight markup (eg. markdown) to HTML
    /// </summary>
    public interface ILightweightMarkupEngine
    {
        /// <summary>
        /// Converts the source string to an HTML string
        /// </summary>
        /// <param name="source">Raw source content</param>
        /// <returns>HTML equivalent transformed</returns>
        string Convert(string source);
    }
}
