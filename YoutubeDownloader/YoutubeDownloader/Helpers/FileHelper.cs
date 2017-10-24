using System;
using System.IO;

namespace YoutubeDownloader
{
    public class FileHelper
    {
        private static readonly string _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string _folderName = "YouTubeDownloader";
        public string Path = System.IO.Path.Combine(_folderPath, _folderName);

        public FileHelper()
        {

        }

        public void CheckIfDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(Path))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(Path);
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
                File.WriteAllBytes(Path + "\\" + fileName, bytes);
            }
            catch (Exception e)
            {

            }
        }
    }
}
