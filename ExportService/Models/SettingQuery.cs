using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    /// <summary>
    /// Настройки выполнения запроса
    /// </summary>
    public class SettingQuery
    {
        /// <summary>
        /// SQL запрос
        /// </summary>
        public string SqlQuery { get; set; }

        /// <summary>
        /// Название файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Название папки таблицы
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Количество записей в одном файле
        /// </summary>
        public int Records { get; set; }
    }
}
