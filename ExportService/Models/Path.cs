using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{

    public class Path
    {
        public string PathStr { get; set; }

        public bool Sftp { get; set; }

        public string Host { get; set; }

        public string Login { get; set; }

        public string Pass { get; set; }
    }
}
