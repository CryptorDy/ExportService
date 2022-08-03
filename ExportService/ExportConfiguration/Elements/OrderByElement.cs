using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class OrderByElement : ConfigurationElement
    {
        [ConfigurationProperty("column", IsKey = true, IsRequired = true)]
        public string Column
        {
            get { return (string)this["column"]; }
            set { this["column"] = value; }
        }

        [ConfigurationProperty("desc", DefaultValue = true, IsRequired = false)]
        public bool Desc
        {
            get { return (bool)this["desc"]; }
            set { this["desc"] = value; }
        }
    }
}
