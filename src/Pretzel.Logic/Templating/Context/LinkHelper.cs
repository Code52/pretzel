using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Templating.Context
{
    [Export]
    public sealed class LinkHelper
    {
        private static readonly Regex TimestampAndTitleFromPathRegex = new Regex(@"\\(?:(?<timestamp>\d+-\d+-\d+)-)?(?<title>[^\\]*)\.[^\.]+$", RegexOptions.Compiled);
        private static readonly Regex CategoryRegex = new Regex(@":category(\d*)", RegexOptions.Compiled);
        private static readonly Regex SlashesRegex = new Regex(@"/{1,}", RegexOptions.Compiled);

        private static readonly string[] HtmlExtensions = new[] { ".markdown", ".mdown", ".mkdn", ".mkd", ".md", ".textile" };

        private static readonly Dictionary<string, string> BuiltInPermalinks = new Dictionary<string, string>
        {
            { "date", "/:categories/:year/:month/:day/:title.html" },
            { "pretty", "/:categories/:year/:month/:day/:title/" },
            { "ordinal", "/:categories/:year/:y_day/:title.html" },
            { "none", "/:categories/:title.html" },
        };

        // http://jekyllrb.com/docs/permalinks/
        public string EvaluatePermalink(string permalink, Page page)
        {
            if (BuiltInPermalinks.ContainsKey(permalink))
            {
                permalink = BuiltInPermalinks[permalink];
            }

            permalink = permalink.Replace(":categories", string.Join("/", page.Categories.ToArray()));
            permalink = permalink.Replace(":dashcategories", string.Join("-", page.Categories.ToArray()));
            permalink = permalink.Replace(":year", page.Date.Year.ToString(CultureInfo.InvariantCulture));
            permalink = permalink.Replace(":month", page.Date.ToString("MM"));
            permalink = permalink.Replace(":day", page.Date.ToString("dd"));
            permalink = permalink.Replace(":title", GetTitle(page.File));
            permalink = permalink.Replace(":y_day", page.Date.DayOfYear.ToString("000"));
            permalink = permalink.Replace(":short_year", page.Date.ToString("yy"));
            permalink = permalink.Replace(":i_month", page.Date.Month.ToString());
            permalink = permalink.Replace(":i_day", page.Date.Day.ToString());

            if (permalink.Contains(":category"))
            {
                var matches = CategoryRegex.Matches(permalink);
                if (matches != null && matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var replacementValue = string.Empty;
                        int categoryIndex;
                        if (match.Success)
                        {
                            if (int.TryParse(match.Groups[1].Value, out categoryIndex) && categoryIndex > 0)
                            {
                                replacementValue = page.Categories.Skip(categoryIndex - 1).FirstOrDefault();
                            }
                            else if (page.Categories.Any())
                            {
                                replacementValue = page.Categories.First();
                            }
                        }

                        permalink = permalink.Replace(match.Value, replacementValue);
                    }
                }
            }

            permalink = SlashesRegex.Replace(permalink, "/");

            return permalink;
        }

        public string EvaluateLink(SiteContext context, Page page)
        {
            var directory = Path.GetDirectoryName(page.Filepath);
            var relativePath = directory.Replace(context.OutputFolder, string.Empty);
            var fileExtension = Path.GetExtension(page.Filepath);

            if (HtmlExtensions.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase))
            {
                fileExtension = ".html";
            }

            var link = relativePath.Replace('\\', '/').TrimStart('/') + "/" + GetPageTitle(page.Filepath) + fileExtension;
            if (!link.StartsWith("/"))
            {
                link = "/" + link;
            }

            return link;
        }

        public string GetTitle(string file)
        {
            return TimestampAndTitleFromPathRegex.Match(file).Groups["title"].Value;
        }

        private string GetPageTitle(string file)
        {
            return Path.GetFileNameWithoutExtension(file);
        }
    }
}
