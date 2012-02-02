﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NDesk.Options;
using Pretzel.Logic;
using Pretzel.Logic.Extensions;

namespace Pretzel.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [CommandInfo(CommandName = "create")]
    public sealed class RecipeCommand : ICommand
    {
        readonly static List<string> Engines = new List<string>(new[] { "Liquid", "Razor" });

        public string Path { get; private set; }
        public string Engine { get; private set; }

        private OptionSet Settings
        {
            get
            {
                return new OptionSet
                           {
                               { "e|engine=", "The render engine", v => Engine = v },
                               { "p|path=", "The path to site directory", p => Path = p },
                           };
            }
        }

        public void Execute(string[] arguments)
        {
            Tracing.Info("create - configure a new site");

            Settings.Parse(arguments);

            var path = String.IsNullOrWhiteSpace(Path)
                             ? Directory.GetCurrentDirectory()
                             : Path;

            var engine = String.IsNullOrWhiteSpace(Engine)
                             ? "Jekyll"
                             : Engine;

            if (!Engines.Any(e => String.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info(String.Format("Requested Render Engine not found: {0}", engine));
                return;
            }

            var recipe = new Recipe(new FileSystem(), engine, path);
            recipe.Create();
        }

        public void WriteHelp(TextWriter writer)
        {
            Settings.WriteOptionDescriptions(writer);
        }
    }
}