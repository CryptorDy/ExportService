using NLog;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export
{
    /// <summary>
    /// Сохранение данных в CSV файле по указанному пути
    /// </summary>
    public class CreateFileInPath : IImplementationData
    {
        /// <summary>
        /// Сервис логгирования
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Cохранение данных
        /// </summary>
        /// <param name="data">Данные для сохранения</param>
        /// <param name="setting">Настройки</param>
        /// <returns>True если сохранение выполнено успешно, иначе False</returns>
        public bool Start(object[][] data, Setting setting)
        {
            logger.Debug($"Начало сохранения данных");

            var path = setting.Path + "/" + setting.Folder;

            //Если указано ограниченное количество записей в одном файле, то определяем количество файлов

            int countRecords;
            int fileCount;
            FillingCounts(out countRecords, out fileCount, setting.Records, data.Length);

            logger.Debug($"Количество файлов {fileCount} по {countRecords} строк в каждом файле");

            string[] recordsFile;

            int dataIndex = 1;
            for (int fileNumber = 0; fileNumber < fileCount; fileNumber++)
            {
                recordsFile = new string[countRecords + 1];
                recordsFile[0] = GetNameColumns(data);

                FillingRecordsFile(ref recordsFile, data, dataIndex);

                string pathFile = path + "/" + GetFileName(fileNumber, setting.FileName);

                //Создание файла
                if (setting.Sftp)
                    CreateInSftp(recordsFile, path, pathFile, setting);
                else
                    CreateInPath(recordsFile, path, pathFile);

            }

            return true;
        }

        /// <summary>
        /// Заполнение массива строк одного файла
        /// </summary>
        /// <param name="recordsFile">Массива строк файла</param>
        /// <param name="data">Источник данных</param>
        /// <param name="dataIndex">Индекс источника данных</param>
        private void FillingRecordsFile(ref string[] recordsFile, object[][] data, int dataIndex)
        {
            for (int j = 1; j < recordsFile.Length; j++)
            {
                recordsFile[j] = RecordToString(data[dataIndex]);
                dataIndex++;

                if (dataIndex == data.Length)
                    break;
            }
        }

        /// <summary>
        /// Создание файла в SFTP
        /// </summary>
        /// <param name="data">Данные для сохранения</param>
        /// <param name="path">Путь для сохранения</param>
        /// <param name="pathFile">Путь с названием файла</param>
        /// <param name="setting">Настройки</param>
        private void CreateInSftp(string[] data, string path, string pathFile, Setting setting)
        {
            SftpClient sftp = new SftpClient(setting.Host, setting.Login, setting.Pass);

            try
            {
                sftp.Connect();
            }
            catch
            {
                logger.Debug($"Ошибка подключения к SFTP серверу");
            }

            if(!sftp.Exists(path))
                sftp.CreateDirectory(path);
            
            UploadFile(sftp, pathFile, data);
           
            sftp.Disconnect();
            
            logger.Debug($"Файл загружен в SFTP по пути {pathFile}");
        }

        /// <summary>
        /// Создание файла в локальном хранилище
        /// </summary>
        /// <param name="data">Данные для сохранения</param>
        /// <param name="pathFile">Путь создания файла</param>
        private void CreateInPath(string[] data, string path, string pathFile)
        {
            CreateDirectory(path);

            //Создание файла
            File.WriteAllLines(pathFile, data, Encoding.UTF8);

            logger.Debug($"Файл загружен по пути {pathFile}");
        }

        /// <summary>
        /// Получить название файла с текущей датой и порядковым номером
        /// </summary>
        /// <param name="fileNumber">Порядковый номер файла</param>
        /// <param name="fileName">Название файла</param>
        /// <returns>Название файла. Пример: users01122022.csv </returns>
        private string GetFileName(int fileNumber, string fileName)
        {
            if (fileNumber == 0)
                return fileName + DateTime.Now.ToString("ddMMyyyy") + ".csv";
            else if (fileNumber > 0)
                return fileName + DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv";
            else
                throw new Exception("Ошибка в индексе файлов");
        }

        /// <summary>
        /// Если нет указаной директории, то создается
        /// </summary>
        /// <param name="path">Путь</param>
        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Заполнение переменных: количество записей одном файле и количество файлов
        /// </summary>
        /// <param name="countRecords">Количество записей в одном файле</param>
        /// <param name="fileCount">Количество файлов</param>
        /// <param name="records">Максимальное количество записей в одном файле</param>
        /// <param name="dataLength">Общее количество строк</param>
        private void FillingCounts(out int countRecords, out int fileCount, int records, int dataLength)
        {
            countRecords = 0;
            fileCount = 0;
            //Если максимальное количество не указано
            if (records <= 0)
            {
                //Все данные в одном файле
                countRecords = dataLength;
                fileCount = 1;
            }
            //Если общее количество строк меньше чем мамксимальное количество записей в одном файле
            else if (records > dataLength)
            {
                countRecords = dataLength;
                fileCount = 1;
            }
            else if (records > 0)
            {
                countRecords = records;
                
                fileCount = CountFiles(dataLength, records);
            }
        }

        /// <summary>
        /// Определяем количество файлов с учетом максимального количества строк для одного файла
        /// </summary>
        /// <param name="dataLength">Общее количество строк</param>
        /// <param name="records">Максимальное количество строк в одном файле</param>
        /// <returns>Количество файлов</returns>
        private int CountFiles(int dataLength, int records)
        {
            //Все записи делим на количество записей в одном файле
            //Минус один чтобы не считать строку с названиями столбцов
            return (int)Math.Ceiling((dataLength - 1) / (double)records);
        }

        /// <summary>
        /// Конвертация массива в одну строку
        /// </summary>
        /// <param name="data">Массив данных одной записи</param>
        /// <returns>Строка разделенная знаком ";"</returns>
        private string RecordToString(object[] data)
        {
            return string.Join(";", data);
        }

        /// <summary>
        /// Возврат названий столбцов в одной стороке
        /// </summary>
        /// <param name="data">Массив данных</param>
        /// <returns>Строка разделенная знаком ";"</returns>
        private string GetNameColumns(object[][] data)
        {
            return string.Join(";", data[0]);
        }

        /// <summary>
        /// Загрузка файлы в SFTP
        /// </summary>
        /// <param name="sftp">Данные для подключения к Sftp</param>
        /// <param name="pathFile">Путь с названием файла</param>
        /// <param name="data">Данные для сохранения</param>
        private void UploadFile(SftpClient sftp, string pathFile, string[] data)
        {
            var bytes = Encoding.Default.GetBytes(string.Join(Environment.NewLine, data));

            var ms = new MemoryStream(bytes);
            sftp.BufferSize = (uint)ms.Length;
            try
            {
                sftp.UploadFile(ms, pathFile);
            }
            catch
            {
                logger.Debug($"Ошибка при загрузке файла в SFTP");
            }
            ms.Dispose();
        }

    }
}
