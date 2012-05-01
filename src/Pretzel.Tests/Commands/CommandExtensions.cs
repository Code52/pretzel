using System.IO;
using System.Text;
using NDesk.Options;
using NSubstitute;
using Pretzel.Logic.Extensibility;
using Xunit;

namespace Pretzel.Tests.Commands
{
    public class CommandExtensions
    {
        [Fact]
        public void help_includes_dynamically_added_commands()
        {
            // arrange
            var stringBuilder = new StringBuilder();
            var optionSet = new OptionSet();
            var extension = Substitute.For<IHaveCommandLineArgs>();
            extension.When(e=>e.UpdateOptions(Arg.Any<OptionSet>()))
                .Do(c=>
                {
                    var options = c.Arg<OptionSet>();

                    options.Add<int>("newOption=", "description", v => NewOption = v);
                });

            // act
            extension.UpdateOptions(optionSet);
            optionSet.WriteOptionDescriptions(new StringWriter(stringBuilder));

            // assert
            var help = stringBuilder.ToString();
            Assert.Contains("--newOption", help);
            Assert.Contains("description", help);
        }

        [Fact]
        public void dynamically_added_option_is_parsed()
        {
            // arrange
            var optionSet = new OptionSet();
            var extension = Substitute.For<IHaveCommandLineArgs>();
            extension.When(e => e.UpdateOptions(Arg.Any<OptionSet>()))
                .Do(c =>
                {
                    var options = c.Arg<OptionSet>();

                    options.Add<int>("newOption=", "description", v => NewOption = v);
                });

            // act
            extension.UpdateOptions(optionSet);
            optionSet.Parse(new[] {"--newOption", "1"});

            // assert
            Assert.Equal(1, NewOption);
        }

        protected int NewOption { get; set; }
    }
}