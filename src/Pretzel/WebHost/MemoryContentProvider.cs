using System;

namespace Pretzel
{
    public class MemoryContentProvider : IWebContent
    {
        private string basePath;

        /// <summary>
        /// Set a base path to work from
        /// </summary>
        /// <param name="path">Base path</param>
        public void SetBasePath(string path)
        {
            basePath = path;
        }

        /// <summary>
        /// See if page is available
        /// </summary>
        /// <param name="request">Request string</param>
        /// <returns>True if available, false otherwise</returns>
        public bool IsAvailable(string request)
        {
            return true;
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Content to show to the user</returns>
        public string GetContent(string request)
        {
            return "Hello world"; // Or something else from the project content maybe
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Content to show to the user</returns>
        public byte[] GetBinaryContent(string request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if request points to a directory
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool IsDirectory(string request)
        {
            return false;
        }

    }
}
