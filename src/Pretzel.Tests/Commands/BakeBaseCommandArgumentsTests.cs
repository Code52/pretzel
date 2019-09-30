using System;
using System.Linq;
using Pretzel.Logic.Commands;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public abstract class BakeBaseCommandArgumentsTests<T> : PretzelBaseCommandArgumentsTests<T>
        where T : BakeBaseCommandArguments
    {
        [Theory]
        [InlineData("-c")]
        [InlineData("--cleantarget")]
        public void CleanTarget(string argument)
        {
            var sut = BuildArguments(argument);

            Assert.True(sut.CleanTarget);
        }  
    }
}
