using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using CsvHelper;

namespace Pretzel.Logic.Templating.Context.DataParsing
{
    internal class CsvTsvDataParser : AbstractDataParser
    {
        public string Delimiter { get; }
        internal CsvTsvDataParser(IFileSystem fileSystem, string extension, string delimiter = ",") : base(fileSystem, extension)
        {
            Delimiter = delimiter;
        }

        public override object Parse(string folder, string method)
        {
            var text = FileSystem.File.ReadAllText(BuildFilePath(folder, method));

            var input = new StringReader(text);

            var csvList = new List<Dictionary<string, object>>();

            using (var csv = new CsvReader(input, new CsvHelper.Configuration.Configuration
            {
                AllowComments = true,
                CountBytes = false,
                CultureInfo = CultureInfo.CurrentCulture,
                Delimiter = Delimiter,
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
                        if (csv.Context.HeaderRecord[i].Contains("."))
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
                    return csvList.First();
                }

                return csvList;
            }
        }
    }
}
