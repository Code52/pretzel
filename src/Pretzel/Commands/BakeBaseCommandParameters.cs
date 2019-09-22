using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO.Abstractions;
using System.Linq;

namespace Pretzel.Commands
{
    public abstract class BakeBaseCommandParameters : PretzelBaseCommandParameters
    {
        protected BakeBaseCommandParameters(IFileSystem fileSystem) : base(fileSystem) { }

        protected override void WithOptions(List<Option> options)
        {
            options.AddRange(new[]
            {
                new Option(new [] { "-c", "--cleantarget" }, "Delete the target directory (_site by default)")
                {
                    Argument = new Argument<bool>()
                },
            });
        }

        public bool CleanTarget { get; set; }
    }
}
