using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class ExecuteByHour : ConfigurationSection
    {
        [ConfigurationProperty("hour", IsRequired = true)]
        public int Hour
        {
            get { return (int)this["hour"]; }
            set { this["hour"] = value; }
        }
    }
}
