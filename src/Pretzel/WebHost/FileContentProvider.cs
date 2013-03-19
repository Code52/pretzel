using System;
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
            basePath = string.IsNullOrWhiteSpace(path) ? Directory.GetCurrentDirectory() : Path.GetFullPath(path);
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
            var file = GetRequestedPage(request);
            return File.Exists(file);
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
            using (var reader = new StreamReader(GetRequestedPage(request)))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="request">Request string</param>
        /// <returns>Filecontents</returns>
        public byte[] GetBinaryContent(string request)
        {
            string fileName = GetRequestedPage(request);

            if (string.IsNullOrEmpty(basePath))
            {
                throw new InvalidOperationException("basePath required");
            }

            byte[] fileContents;
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                var fileInfo = new FileInfo(fileName);
                fileContents = reader.ReadBytes((int)fileInfo.Length);
            }

            return fileContents;
        }

    	/// <summary>
    	/// Get the path for the page to send to the user
    	/// </summary>
    	/// <param name="request"> </param>
    	/// <returns>Path to file</returns>
    	private string GetRequestedPage(string request)
        {
			string requestString = basePath + request;

            if (requestString.EndsWith("/", StringComparison.Ordinal))
            {
                // Load index.html as default
                requestString = Path.Combine(requestString, "index.html");
            }

            return requestString;
        }
    }
}
