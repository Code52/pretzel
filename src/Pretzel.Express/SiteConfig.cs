namespace Pretzel
{
    public class SiteConfig
    {
        public SiteConfig (){}

        public SiteConfig(string dir, int port, bool running)
        {
            Port = port;
            Directory = dir;
            Running = running;
        }
        public int Port { get; set; }
        public string Directory { get; set; }
        public bool Running { get; set; }
    }
}