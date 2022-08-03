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
        [ConfigurationProperty("condition", IsRequired = true)]
        public string Condition
        {
            get { return (string)this["condition"]; }
            set { this["condition"] = value; }
        }

        [ConfigurationProperty("operator", DefaultValue = "AND", IsRequired = false)]
        public string Operator
        {
            get { return (string)this["operator"]; }
            set { this["operator"] = value; }
        }

    }
}
