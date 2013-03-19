using IFileSystem = System.IO.Abstractions.IFileSystem;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Loggers;
using dotless.Core.Parser;
using dotless.Core.Stylizers;

namespace Pretzel.Tests.Minification
{
    public class TestContainerFactory 
    {
        public ILessEngine GetEngine(IFileSystem fileSystem, string directory)
        {
            IStylizer stylizer = new HtmlStylizer();

            IPathResolver pathResolver = new TestPathResolver(directory);
            IFileReader reader = new TestFileReader(fileSystem, pathResolver);
            var importer = new Importer(reader);
            var parser = new Parser(stylizer, importer);
            ILogger logger = new ConsoleLogger(LogLevel.Error);
            var engine = new LessEngine(parser, logger, true);
            engine.Compress = true;
            return engine;
        }
    }
}