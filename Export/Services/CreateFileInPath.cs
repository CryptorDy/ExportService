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

        public bool Start(object[][] data, Setting setting)
        {
            var path = setting.Path + "\\" + setting.Folder;
            var pathSFTP = setting.Path;
            var fileCount = 0;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            if (setting.Records == 0)
                fileCount = 1;
            else
                fileCount = (int)Math.Ceiling((data.Length - 1) / (double)setting.Records);


            var countRecords = setting.Records == 0 ? data.Length : setting.Records;
            string[] records;

            int k = 1;
            for (int i = 0; i < fileCount; i++)
            {
                records = new string[countRecords + 1];
                records[0] = string.Join(";", data[0]);

                for (int j = 1; j < records.Length; j++)
                {
                    var record = string.Join(";", data[k]);
                    records[j] = record;
                    k++;

                    if (k == data.Length)
                        break;
                }
                if (setting.Sftp)
                    CreateToSftp(records, i, path, setting.FileName);
                else
                    CreateToPath(records, i, path, setting.FileName);

            }

            return true;
        }



        private void CreateToSftp(string[] records, int fileNumber, string path, string fileName)
        {

            string remoteDirectory = "/";

            string host = @"10.100.0.57";
            string username = "esn";
            string password = "QQRs5hh1jz";

            //using (SftpClient sftp = new SftpClient(setting.Host, setting.Login, setting.Pass))
            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();

                    var bytes = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, records));

                    if (fileNumber == 0)
                    {
                      

                        using (var ms = new MemoryStream(bytes))
                        {
                            sftp.BufferSize = (uint)ms.Length;
                            //ms.Position = 0;
                            sftp.UploadFile(ms, fileName + DateTime.Now.ToString("ddMMyyyy") + ".csv");
                        }

                        using (Stream fileStream = File.Create(path + "\\" + fileName + DateTime.Now.ToString("ddMMyyyy") + ".csv"))
                        {
                            sftp.DownloadFile(remoteDirectory + fileName + DateTime.Now.ToString("ddMMyyyy") + ".csv", fileStream);
                        }
                    }

                    else if (fileNumber > 0)
                    {

                        using (var ms = new MemoryStream(bytes))
                        {
                            sftp.BufferSize = (uint)ms.Length;
                            //ms.Position = 0;
                            sftp.UploadFile(ms, fileName + DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv");
                        }

                        using (Stream fileStream = File.Create(path + "\\" + fileName + DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv"))
                        {
                            sftp.DownloadFile(remoteDirectory + fileName + DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv", fileStream);
                        }
                    }

                    sftp.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }
            }
        }

        private void CreateToPath(string[] records, int fileNumber, string path, string fileName)
        {
            if (fileNumber == 0)
                File.WriteAllLines(path + "\\" + fileName + DateTime.Now.ToString("ddMMyyyy") + ".csv", records, Encoding.UTF8);

            else if (fileNumber > 0)
                File.WriteAllLines(path + "\\" + fileName + DateTime.Now.ToString("ddMMyyyy") + "_" + (fileNumber + 1) + ".csv", records, Encoding.UTF8);
        }

    }
}
