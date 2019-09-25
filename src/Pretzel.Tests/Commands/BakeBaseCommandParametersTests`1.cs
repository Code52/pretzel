using System;
using System.Linq;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public abstract class BakeBaseCommandParametersTests<T> : PretzelBaseCommandParametersTests<T>
        where T : BakeBaseCommandParameters
    {
        [Theory]
        [InlineData("-c")]
        [InlineData("--cleantarget")]
        public void CleanTarget(string argument)
        {
            var sut = BuildParameters(argument);

            Assert.True(sut.CleanTarget);
        }  
    }
}
