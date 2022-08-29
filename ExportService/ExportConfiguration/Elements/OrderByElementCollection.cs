using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    [ConfigurationCollection(typeof(OrderByElement), AddItemName = "item")]
    public class OrderByElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OrderByElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OrderByElement)element).Column;
        }

    }
}
