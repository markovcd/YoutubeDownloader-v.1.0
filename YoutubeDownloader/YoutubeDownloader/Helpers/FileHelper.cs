using System;
using System.IO;

namespace YoutubeDownloader
{
    public class FileHelper
    {
        private static readonly string _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string _folderName = "YouTubeDownloader";
        private string _path = Path.Combine(_folderPath, _folderName);

        public FileHelper()
        {

        }

        public void CheckIfDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_path))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(_path);
                }
            }
            catch (IOException e)
            {

            }
        }

        public void WriteToFile(string fileName, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(_path + "\\" + fileName, bytes);
            }
            catch (Exception e)
            {

            }
        }
    }
}
