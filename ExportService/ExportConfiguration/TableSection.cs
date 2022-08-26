using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class TableSection : ConfigurationSection
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("folder", IsRequired = false)]
        public string Folder
        {
            get { return (string)this["folder"]; }
            set { this["folder"] = value; }
        }

        [ConfigurationProperty("select", IsRequired = false)]
        public string Select
        {
            get { return (string)this["select"]; }
            set { this["select"] = value; }
        }

        [ConfigurationProperty("limit", DefaultValue = 0, IsRequired = false)]
        public int? Limit
        {
            get { return (int?)this["limit"]; }
            set { this["limit"] = value; }
        }

        [ConfigurationProperty("file")]
        public FileElement File
        {
            get { return (FileElement)this["file"]; }
            set { this["file"] = value; }
        }

        [ConfigurationProperty("where")]
        public WhereElementCollection Where
        {
            get { return (WhereElementCollection)this["where"]; }
            set { this["where"] = value; }
        }

        [ConfigurationProperty("orderBy")]
        public OrderByElementCollection OrderBy
        {
            get { return (OrderByElementCollection)this["orderBy"]; }
            set { this["orderBy"] = value; }
        }
        
    }
}
