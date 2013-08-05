using System.Collections.Generic;
using System.Configuration;

namespace Pretzel
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class SiteSettings
    {
        public List<SiteConfig> Sites { get; set; }
    }
}