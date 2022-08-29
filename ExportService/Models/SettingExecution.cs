using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportService
{
    /// <summary>
    /// Настройки выполнения эспорта данных
    /// </summary>
    public class SettingExecution
    {
        /// <summary>
        /// Выполнение по дням в указанное время
        /// </summary>
        public bool IsExecuteByDay { get; set; }

        /// <summary>
        /// Настройки выполнения по дням
        /// </summary>
        public ExecuteByDay ExecuteByDay { get; set; }

        /// <summary>
        /// Выполнение в интервале по часам
        /// </summary>
        public bool IsExecuteByHour { get; set; }

        /// <summary>
        /// Настройки выполнения по часам
        /// </summary>
        public ExecuteByHour ExecuteByHour { get; set; }

        /// <summary>
        /// Выполняем экспорт сразу при запуске сервиса
        /// </summary>
        public NowExport NowExport { get; set; }

        /// <summary>
        /// Настройки пути в котором экспортируем
        /// </summary>
        public ExportPath ExportPath { get; set; }

    }
}
