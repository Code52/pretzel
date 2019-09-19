using System.Collections.Generic;
using DotLiquid;
using Pretzel.Logic.Extensions;
using System.IO;
using System.IO.Abstractions;
using System.Collections;
using System;
using YamlDotNet.RepresentationModel;
using CsvHelper;
using System.Linq;
using System.Globalization;
using System.Text;

namespace Pretzel.Logic.Templating.Context
{
    public class Data : Drop
    {
        private readonly IFileSystem fileSystem;
        private readonly string dataDirectory;
        private readonly Dictionary<string, System.Lazy<object>> cachedResults = new Dictionary<string, Lazy<object>>();

        public Data(IFileSystem fileSystem, string dataDirectory)
        {
            this.fileSystem = fileSystem;
            this.dataDirectory = dataDirectory;
        }

        public override object this[object method]
        {
            get
            {
                var res = base[method];
                if (res != null)
                {
                    return res;
                }

                if (!cachedResults.ContainsKey(method.ToString()))
                {
                    var cachedResult = new Lazy<object>(() =>
                    {
                        if (!fileSystem.Directory.Exists(dataDirectory))
                        {
                            return null;
                        }

                        object result;

                        if (TryParseYaml(dataDirectory, method.ToString(), out result))
                        {
                            return result;
                        }

                        if (TryParseYaml(dataDirectory, method.ToString(), out result, "json"))
                        {
                            return result;
                        }

                        if (TryParseCsv(dataDirectory, method.ToString(), out result))
                        {
                            return result;
                        }

                        if (TryParseCsv(dataDirectory, method.ToString(), out result, "tsv", "\t"))
                        {
                            return result;
                        }

                        var subFolder = Path.Combine(dataDirectory, method.ToString());
                        if (fileSystem.Directory.Exists(subFolder))
                        {
                            return new Data(fileSystem, subFolder);
                        }

                        return null;
                    });
                    cachedResults[method.ToString()] = cachedResult;
                    return cachedResult.Value;
                }

                return cachedResults[method.ToString()].Value;
            }
        }

        bool TryParseYaml(string folder, string methodName, out object yamlResult, string ext = "yml")
        {
            var yamlFileName = Path.Combine(folder, $"{methodName}.{ext}");
            if (fileSystem.File.Exists(yamlFileName))
            {
                var text = fileSystem.File.ReadAllText(yamlFileName);

                var input = new StringReader(text);

                var yaml = new YamlStream();
                yaml.Load(input);

                if (yaml.Documents.Count == 0)
                {
                    yamlResult = null;
                    return false;
                }

                var root = yaml.Documents[0].RootNode;
                if (root is YamlSequenceNode seq)
                {
                    yamlResult = seq;
                    return true;
                }

                yamlResult = text.ParseYaml();
                return yamlResult != null;
            }
            yamlResult = null;
            return false;
        }

        bool TryParseCsv(string folder, string methodName, out object csvResult, string ext = "csv", string delimiter = ",")
        {
            var csvFileName = Path.Combine(folder, $"{methodName}.{ext}");
            if (fileSystem.File.Exists(csvFileName))
            {
                var text = fileSystem.File.ReadAllText(csvFileName);

                var input = new StringReader(text);

                var csvList = new List<Dictionary<string, object>>();

                using (var csv = new CsvReader(input, new CsvHelper.Configuration.Configuration
                {
                    AllowComments = false,
                    CountBytes = false,
                    CultureInfo = CultureInfo.CurrentCulture,
                    Delimiter = delimiter,
                    DetectColumnCountChanges = false,
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                    IgnoreQuotes = false,
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    TrimOptions = CsvHelper.Configuration.TrimOptions.None,
                    BadDataFound = null,
                }))
                {
                    var isHeader = true;
                    while (csv.Read())
                    {
                        if (isHeader)
                        {
                            csv.ReadHeader();
                            isHeader = false;
                            continue;
                        }

                        if (string.IsNullOrEmpty(csv.GetField(0)))
                        {
                            isHeader = true;
                            continue;
                        }

                        var csvRow = new Dictionary<string, object>();

                        for (int i = 0; i < csv.Context.HeaderRecord.Length; i++)
                        {
                            if(csv.Context.HeaderRecord[i].Contains("."))
                            {
                                var currentDictionary = new Dictionary<string, object>();
                                var tree = csv.Context.HeaderRecord[i].Split('.');
                                var firstKey = tree.First();
                                foreach (var subObject in tree.Skip(1))
                                {

                                    var newDict = new Dictionary<string, object>();
                                    currentDictionary[subObject] = newDict;
                                    currentDictionary = newDict;
                                }
                                currentDictionary[tree.Last()] = csv.GetField(i);
                                csvRow[firstKey] = currentDictionary;
                            }
                            else
                            {
                                var field = csv.GetField(i);
                                csvRow[csv.Context.HeaderRecord[i]] = field;
                            }
                        }

                        csvList.Add(csvRow);
                    }

                    if (csvList.Count == 1)
                    {
                        csvResult = csvList.First();
                        return true;
                    }

                    csvResult = csvList;
                    return true;
                }
            }
            csvResult = null;
            return false;
        }
    }
}
