using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    public interface IExport
    {
        bool Execute(string sqlQuery, Setting settingQuery);
    }
}
