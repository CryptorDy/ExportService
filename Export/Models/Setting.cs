using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    /// <summary>
    /// Настройки экспорта
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Путь по которому сохраняется файл
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Название файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Название папки
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Количество строк в одном файле
        /// </summary>
        public int Records { get; set; }

        /// <summary>
        /// Общее количество строк
        /// </summary>
        public int DataCount { get; set; }

        /// <summary>
        /// Включен sftp
        /// </summary>
        public bool Sftp { get; set; }

        /// <summary>
        /// Хост sftp
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Login sftp
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// password sftp
        /// </summary>
        public string Pass { get; set; }
    }
}
