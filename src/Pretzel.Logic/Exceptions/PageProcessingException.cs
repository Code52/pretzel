using System;
using System.Runtime.Serialization;

namespace Pretzel.Logic.Exceptions
{
    [Serializable]
    public class PageProcessingException : Exception
    {
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp


        public PageProcessingException()
        {
        }

        public PageProcessingException(string message)
            : base(message)
        {
        }

        public PageProcessingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PageProcessingException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}