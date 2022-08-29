using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    /// <summary>
    /// Сервис для получения данных по sql запросу
    /// </summary>
    public interface IExport
    {
        /// <summary>
        /// Выполнение экспорта
        /// </summary>
        /// <param name="sqlQuery">SQL запрос для экспорта данных</param>
        /// <param name="settingQuery">Настройки экспорта</param>
        /// <returns>True если экспорт выполнен успешно, иначе False</returns>
        bool Execute(string sqlQuery, Setting settingQuery);
    }
}
