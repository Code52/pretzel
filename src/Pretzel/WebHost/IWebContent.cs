﻿using System;

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