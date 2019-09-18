using NDesk.Options;

namespace Pretzel.Logic.Extensibility
{
    public interface IHaveCommandLineArgs
    {
        void UpdateOptions(OptionSet options);
        string[] GetArguments(string command);
    }
}
