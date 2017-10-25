using System;
using System.Diagnostics;
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
                Debug.WriteLine("Exception occured: {0}", e.ToString());
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
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
        }

        public void RemoveFile(string fileName)
        {
            try
            {
                if (CheckPossibleDuplicate(fileName))
                {
                    File.Delete(Path + "\\" + fileName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
        }

        public void RenameFile(string oldNamePath, string newNamePath)
        {
            try
            {
                File.Move(oldNamePath, newNamePath);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
        }

        public bool CheckPossibleDuplicate(string fileName)
        {
            return File.Exists(System.IO.Path.Combine(Path, fileName));
        }
    }
}
