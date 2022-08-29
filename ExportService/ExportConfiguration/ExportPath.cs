using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    public class ExportPath : ConfigurationSection
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("sftp", DefaultValue = "false", IsRequired = false)]
        public bool Sftp
        {
            get { return (bool)this["sftp"]; }
            set { this["sftp"] = value; }
        }

        [ConfigurationProperty("host", IsRequired = false)]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("login", IsRequired = false)]
        public string Login
        {
            get { return (string)this["login"]; }
            set { this["login"] = value; }
        }

        [ConfigurationProperty("pass", IsRequired = false)]
        public string Pass
        {
            get { return (string)this["pass"]; }
            set { this["pass"] = value; }
        }
    }
}
