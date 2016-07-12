using System.Collections.Generic;
using Pretzel.Logic.Extensions;
using Xunit;

namespace Pretzel.Tests.Extensions
{
    public class DictionaryExtensionTests
    {
        [Fact]
        public void Merge_two_dictionaries_returns_new_merged_dictionary()
        {
            var first = new Dictionary<string, string>
            {
                { "A", "a-first" },
                { "B", "b-first" },
            };
            var second = new Dictionary<string, string>
            {
                { "A", "a-second" },
                { "C", "c-second" },
            };

            var merged = first.Merge(second);

            Assert.Equal(3, merged.Count);
            Assert.Equal("a-second", merged["A"]);
            Assert.Equal("b-first", merged["B"]);
            Assert.Equal("c-second", merged["C"]);
        }

        [Fact]
        public void Merge_with_null_returns_copy_of_first()
        {
            var first = new Dictionary<string, string>
            {
                { "A", "a-first" },
                { "B", "b-first" },
            };

            var merged = first.Merge(null);

            Assert.NotSame(merged, first);
            Assert.Equal(2, merged.Count);
            Assert.Equal("a-first", merged["A"]);
            Assert.Equal("b-first", merged["B"]);
        }
    }
}
