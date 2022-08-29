using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    [ConfigurationCollection(typeof(WhereElement), AddItemName = "item")]
    public class WhereElementCollection : ConfigurationElementCollection
    {
        private static readonly ObjectIDGenerator idGenerator = new ObjectIDGenerator();

        protected override ConfigurationElement CreateNewElement()
        {
            return new WhereElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            bool firstTime;
            return idGenerator.GetId(element, out firstTime);
        }

    }
}
