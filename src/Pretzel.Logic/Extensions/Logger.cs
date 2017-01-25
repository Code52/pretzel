using System;
using System.Collections.Generic;
using System.IO;

namespace Pretzel.Logic.Extensions
{
    [Obsolete("Use Tracing instead")]
    public class Logger
    {
        private readonly List<Tracing.Category> categories;
        private TextWriter writer;

        public Logger()
        {
            categories = new List<Tracing.Category>();
        }

        public void SetWriter(TextWriter textWriter)
        {
            writer = textWriter;
        }
        
        public void AddCategory(Tracing.Category category)
        {
            categories.Add(category);
        }

        public void Write(string message, Tracing.Category category)
        {
            if (writer == null) return;

            if (categories.Contains(category))
            {
                writer.WriteLine(message);
                if (category == Tracing.Category.Error) 
                {
                    writer.Flush();
                }
            }
        }

        internal void WriteFormat(string message, Tracing.Category category, params object[] messageParameters)
        {
            Write(string.Format(message, messageParameters), category);
        }
    }
}