using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Extensions
{
    public class Logger
    {
        private readonly List<string> categories;
        private TextWriter writer;

        public Logger()
        {
            categories = new List<string>();
        }

        public void SetWriter(TextWriter textWriter)
        {
            writer = textWriter;
        }
        
        public void AddCategory(string category)
        {
            categories.Add(category);
        }

        public void Write(string message, string category)
        {
            if (writer == null) return;

            if (categories.Contains(category))
                writer.WriteLine(message);
        }
    }
}