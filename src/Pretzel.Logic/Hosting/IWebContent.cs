using System;

namespace Pretzel.Logic.Hosting
{
    public interface IWebContent
    {
        void SetBasePath(string path);
        bool IsAvailable(string request);
        bool IsDirectory(string request);
        string GetContent(string request);
        byte[] GetBinaryContent(string request);
    }
}
