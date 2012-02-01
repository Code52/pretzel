using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pretzel
{
    public class FileContentProvider : IWebContent
    {
        private string basePath;

        /// <summary>
        /// Set a base path to work from
        /// </summary>
        /// <param name="path">Base path</param>
        public void SetBasePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // No path specified, get working directory
                this.basePath = Directory.GetCurrentDirectory();
            }
            else
            {
                // Get an absolute path for the directory
                this.basePath = Path.GetFullPath(path);
            }
        }

        /// <summary>
        /// Check if file is found
        /// </summary>
        /// <param name="request">Request string</param>
        /// <returns>True if file is found, false otherwise</returns>
        public bool IsAvailable(string request)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                throw new InvalidOperationException("basePath required");
            }

            // Tell caller whether the file exists or not
            string file = GetRequestedPage(request);
            if (File.Exists(file))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="request">Request string</param>
        /// <returns>Filecontents</returns>
        public string GetContent(string request)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                throw new InvalidOperationException("basePath required");
            }

            string fileContents;
            using (StreamReader reader = new StreamReader(GetRequestedPage(request)))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }

        /// <summary>
        /// Get the path for the page to send to the user
        /// </summary>
        /// <param name="env">env-dictionary as sent from the server-callback</param>
        /// <returns>Path to file</returns>
        private string GetRequestedPage(string request)
        {
            string requestString;

            requestString = basePath + request;

            if (requestString.EndsWith("/", StringComparison.Ordinal))
            {
                // Load index.html as default
                requestString = Path.Combine(requestString, "index.html");
            }

            return requestString;
        }
    }
}
