using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pretzel
{
    public interface IWebContent
    {
        void SetBasePath(string path);
        bool IsAvailable(string request);
        string GetContent(string request);
        byte[] GetBinaryContent(string request);
    }
}
