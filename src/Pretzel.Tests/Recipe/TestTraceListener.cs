using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pretzel.Tests.Recipe
{
    public class TestTraceListener : TraceListener
    {
        public TestTraceListener()
        {
            Messages = new List<string>();
        }

        public List<string> Messages { get; set; }

        public override void Write(string message)
        {
            
        }

        public override void WriteLine(string message)
        {
            Messages.Add(message);
        }

        public bool Received(string text)
        {
            return Messages.Any(t => string.CompareOrdinal(t, text) == 0);
        }
    }
}