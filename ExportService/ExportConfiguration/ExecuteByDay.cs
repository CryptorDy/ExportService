using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class ExecuteByDay : ConfigurationSection
    {
        [ConfigurationProperty("day", IsRequired = true)]
        public int Day
        {
            get { return (int)this["day"]; }
            set { this["day"] = value; }
        }

        [ConfigurationProperty("time", IsRequired = true)]
        public string Time
        {
            get { return (string)this["time"]; }
            set { this["time"] = value; }
        }
    }
}
