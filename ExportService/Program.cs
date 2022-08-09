using Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace ExportService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            var container = new UnityContainer();
            container.RegisterType<IQuery, QueryOracle>();
            container.RegisterType<IImplementationData, CreateFileInPath>();
            var export = container.Resolve<Export.Export>();

            container.RegisterInstance<IExport>(export);

#if DEBUG
            container.Resolve<Service>();
            while (true)
                System.Threading.Thread.Sleep(2000);
#else
            ServiceBase[] services = new ServiceBase[1]
			{
				container.Resolve<Service>()
			};
			ServiceBase.Run(services);
#endif
        }
    }
}
