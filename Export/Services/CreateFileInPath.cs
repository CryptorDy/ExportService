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
            if (setting.Sftp)
                CreateToSftp(data, setting);
            else
                CreateToPath(data, setting);

            return true;
        }


        private void CreateToPath(object[][] data, Setting setting)
        {
            var index = 1;
            while (true)
            {
                if (Directory.Exists(setting.Path + "\\" + setting.Folder))
                    Directory.CreateDirectory(setting.Path + "\\" + setting.Folder + "()");

                index++;
            }
            

            using (FileStream fs = File.Create(setting.Path))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }

        private void CreateToSftp(object[][] data, Setting setting)
        {
            string host = @"141.8.192.151";
            string username = "f0682093";
            string password = "uzpeuhxain";

            string remoteDirectory = "\\";

            using (SftpClient sftp = new SftpClient(host, username, password))
            {
                try
                {
                    sftp.Connect();

                    var files = sftp.ListDirectory(remoteDirectory);
                    string s = null;

                    //var destinationFile = Path.Combine(" ", "ff");
                    //using (var fs = new FileStream(destinationFile, FileMode.Create)) 
                    //{
                    //    sftp.DownloadFile(remoteDirectory, fs);
                    //}

                    foreach (var file in files)
                    {
                        Console.WriteLine(file.Name);
                    }

                    sftp.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception has been caught " + e.ToString());
                }
            }
        }

        public void WriteToFile(string[] data)
        {
            var dataByte = GetBytes(data);

        }

        static byte[] GetBytes(string[] values)
        {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        
    }
}
