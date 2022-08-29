using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class WhereElement : ConfigurationElement
    {
        [ConfigurationProperty("column", IsRequired = true)]
        public string Column
        {
            get { return (string)this["column"]; }
            set { this["column"] = value; }
        }

        [ConfigurationProperty("operator", DefaultValue = "=", IsRequired = true)]
        public string Operator
        {
            get { return (string)this["operator"]; }
            set { this["operator"] = value; }
        }

        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }

        [ConfigurationProperty("and", DefaultValue = false, IsRequired = false)]
        public bool And
        {
            get { return (bool)this["and"]; }
            set { this["and"] = value; }
        }

    }
}
