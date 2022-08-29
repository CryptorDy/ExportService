using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class NowExport : ConfigurationSection
    {

        [ConfigurationProperty("on", DefaultValue = true, IsRequired = true)]
        public bool On
        {
            get { return (bool)this["on"]; }
            set { this["on"] = value; }
        }

    }
}
