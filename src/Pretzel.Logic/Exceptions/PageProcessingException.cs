using System;
using System.Runtime.Serialization;

namespace Pretzel.Logic.Exceptions
{
    [Serializable]
    public class PageProcessingException : Exception
    {
        internal PageProcessingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}