using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    /// <summary>
    /// Экспорт данных
    /// </summary>
    public class Export : IExport
    {

        /// <summary>
        /// Сервис для получения данных по sql запросу
        /// </summary>
        private readonly IQuery query;

        /// <summary>
        /// Сервис для реализации данных из БД
        /// </summary>
        private readonly IImplementationData implementationData;

        public Export(IQuery query, IImplementationData implementationData)
        {
            this.query = query;
            this.implementationData = implementationData;
        }


        /// <summary>
        /// Выполнение экспорта данных
        /// </summary>
        /// <param name="settingQuery">SQL запрос</param>
        /// <param name="exportPath">Настройки для выгрузки данных</param>
        /// <returns>True - если выгрузка удалась успешно, иначе False</returns>
        public bool Execute(string sqlQuery, Setting setting)
        {
            var result = query.Execute(sqlQuery, setting.DataCount);

            if (result is null) throw new Exception("Запрос не вернул данные");

            var imp = implementationData.Start(result, setting);

            if (!imp) throw new Exception("Не удалось выгрузить данные");

            return imp;
        }
    }
}
