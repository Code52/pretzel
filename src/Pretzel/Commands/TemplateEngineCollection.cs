using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Pretzel.Logic.Templating;

namespace Pretzel.Commands
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class TemplateEngineCollection : IPartImportsSatisfiedNotification
    {
        [ImportMany] 
#pragma warning disable 649
        private Lazy<ISiteEngine, ISiteEngineInfo>[] templateEngineMap;
#pragma warning restore 649

        public Dictionary<string, ISiteEngine> Engines { get; private set; }

        public ISiteEngine this[string name]
        {
            get
            {
                ISiteEngine engine;
                Engines.TryGetValue(name.ToLower(), out engine);
                return engine;
            }
        }

        public void OnImportsSatisfied()
        {
            Engines = new Dictionary<string, ISiteEngine>(templateEngineMap.Length);

            foreach (var command in templateEngineMap)
            {
                if (!Engines.ContainsKey(command.Metadata.Engine))
                    Engines.Add(command.Metadata.Engine, command.Value);
            }
        }
    }
}