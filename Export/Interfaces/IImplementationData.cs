using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    public interface IImplementationData
    {
        bool Start(object[][] data, Setting setting);
    }
}
