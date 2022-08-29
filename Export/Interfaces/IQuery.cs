using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    public interface IQuery
    {
        object[][] Execute(string query, int dataCount);
    }
}
