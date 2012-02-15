using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Pretzel.Logic.Import
{
    public class XhtmlToMarkdownConverter
    {
        public static string Convert(string html)
        {
            // going to use an XSLT by Andrew Green (used with permission)
            // http://www.lowerelement.com/Geekery/XML/XHTML-to-Markdown.html
            
            using (StringReader sReader = new StringReader(html))
            using (XmlReader xReader = XmlReader.Create(sReader))
            using (StringWriter sWriter = new StringWriter())
            {
                XslCompiledTransform xslTransform = new XslCompiledTransform();
                var xslStream = GetEmbeddedResourceStream("Pretzel.Logic.Import.markdown.xsl");
                xslTransform.Load(XmlReader.Create(xslStream));
                xslTransform.Transform(xReader, null, sWriter);
                return sWriter.ToString();
            }
        }

        public static Stream GetEmbeddedResourceStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

    }
}
