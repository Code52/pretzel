using System;
using Pretzel.Logic.Exceptions;
using Xunit;

namespace Pretzel.Tests.Exceptions
{
    public class PageProcessingExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_IsCorrectStructure()
        {
            var expected = "Some Message";
            var ex = new PageProcessingException(expected);
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void Constructor_WithInnerAndMessage_IsCorrectStructure()
        {
            var expected = "Some Message";
            var inner = new Exception();
            var ex = new PageProcessingException(expected, inner);
            Assert.Equal(expected, ex.Message);
            Assert.Same(inner, ex.InnerException);
        }
    }
}
