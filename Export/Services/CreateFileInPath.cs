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
    public class CreateFileInPath : IImplementationData
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool Start(object[][] data, Setting setting)
        {
            logger.Debug($"Начало сохранения данных по пути {setting.Path}");

            var path = setting.Path + "\\" + setting.Folder;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //Если указано ограниченное количество записей в одном файле, то определяем количество файлов

            var countRecords = 0;
            int fileCount = 0;
            if (setting.Records <= 0)
            {
                countRecords = data.Length;
                fileCount = 1;
            } else if (setting.Records > data.Length)
            {
                countRecords = data.Length;
                fileCount = 1;
            } else if(setting.Records > 0)
            {
                countRecords = setting.Records;
                //Все записи делим на количество записей в одном файле
                //Минус один чтобы не считать строку с названиями столбцов
                fileCount = (int)Math.Ceiling((data.Length - 1) / (double)setting.Records);
            }

            logger.Debug($"Количество файлов {fileCount} по {countRecords} строк в каждом файле");

            string[] records;

            int dataIndex = 1;
            for (int i = 0; i < fileCount; i++)
            {
                records = new string[countRecords + 1];
                records[0] = GetNameTables(data);

                for (int j = 1; j < records.Length; j++)
                {
                    records[j] = RecordToString(data[dataIndex]);
                    dataIndex++;

                    if (dataIndex == data.Length)
                        break;
                }

                //Создание файла
                if (setting.Sftp)
                    CreateToSftp(records, i, path, setting.FileName, setting);
                else
                    CreateToPath(records, i, path, setting.FileName);

            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="fileNumber"></param>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <param name="setting"></param>
        /// <exception cref="Exception"></exception>
        private void CreateToSftp(string[] records, int fileNumber, string path, string fileName, Setting setting)
        {
            SftpClient sftp = new SftpClient(setting.Host, setting.Login, setting.Pass);
            
            try
            {
                sftp.Connect();
            }
            catch
            {
                throw new Exception("Ошибка подключения к SFTP серверу");
            }

            var bytes = Encoding.Default.GetBytes(string.Join(Environment.NewLine, records));

            using (var ms = new MemoryStream(bytes))
            {
                //Если записи деляться на несколько файлов, то сохраняем с нумерацией в названии файла
                if (fileNumber == 0)
                {
                    fileName += DateTime.Now.ToString("ddMMyyyy") + ".csv";
                    UploadFile(sftp, ms, setting.Folder +"/"+ fileName);
                    logger.Debug($"Файл {fileName} загружен в SFTP по пути {setting.Folder}");
                }
                else if (fileNumber > 0)
                {
                    fileName += DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv";
                    UploadFile(sftp, ms, setting.Folder + "/" + fileName);
                    logger.Debug($"Файл {fileName} загружен в SFTP по пути {setting.Folder}");
                }
            }

            sftp.Disconnect();
        }

        private void CreateToPath(string[] records, int fileNumber, string path, string fileName)
        {
            if (fileNumber == 0)
                File.WriteAllLines(path + "\\" + fileName + DateTime.Now.ToString("ddMMyyyy") + ".csv", records, Encoding.UTF8);

            else if (fileNumber > 0)
                File.WriteAllLines(path + "\\" + fileName + DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv", records, Encoding.UTF8);
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
        /// Возврат названий таблицы в одной стороке
        /// </summary>
        /// <param name="data">Массив данных</param>
        /// <returns>Строка разделенная знаком ";"</returns>
        private string GetNameTables(object[][] data)
        {
            return string.Join(";", data[0]);
        }

        /// <summary>
        /// Загрузка файлы в SFTP
        /// </summary>
        /// <param name="sftp"></param>
        /// <param name="ms"></param>
        /// <param name="fileName"></param>
        private void UploadFile(SftpClient sftp, MemoryStream ms, string fileName)
        {
            try
            {
                sftp.BufferSize = (uint)ms.Length;
                sftp.UploadFile(ms, fileName);
            }
            catch
            {
                throw new Exception("Ошибка при загрузке файла в SFTP");
            }
            
        }

    }
}
