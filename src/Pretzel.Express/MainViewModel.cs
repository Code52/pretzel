using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Pretzel.Annotations;
using Pretzel.Commands;

namespace Pretzel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        [Import]
        public CommandCollection Commands { get; set; }
        private CompositionContainer container;

        private readonly List<int> portList = new List<int>();

        public ObservableCollection<Site> Sites { get; set; }
        public MainViewModel()
        {
            Sites = new ObservableCollection<Site>();
        }

        public void StartNewSite(string directory, int port = 8080)
        {
            while (portList.Contains(port))
                port++;

            portList.Add(port);

            var site = new Site();
            container.SatisfyImportsOnce(site);          
            site.Directory = directory;
            site.Port = port;
            site.Execute();
            Sites.Add(site);

            
        }

        public void Compose()
        {
            try
            {
                var first = new AggregateCatalog();
                first.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                first.Catalogs.Add(new AssemblyCatalog(typeof(Logic.SanityCheck).Assembly));
                container = new CompositionContainer(first);

                var batch = new CompositionBatch();
                batch.AddExportedValue<IFileSystem>(new FileSystem());
                batch.AddPart(this);
                container.Compose(batch);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(@"Unable to load: \r\n{0}",
                    string.Join("\r\n", ex.LoaderExceptions.Select(e => e.Message)));

                throw;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}