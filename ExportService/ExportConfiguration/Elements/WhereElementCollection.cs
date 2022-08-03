using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    [ConfigurationCollection(typeof(WhereElement), AddItemName = "item")]
    public class WhereElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WhereElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WhereElement)element).Condition;
        }

    }
}
