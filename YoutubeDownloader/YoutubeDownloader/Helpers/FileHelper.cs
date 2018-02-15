using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace YoutubeDownloader
{
    public sealed class FileHelper
    {
        #region Fields & Properties
        private static readonly string _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string _youtubeLastPartString = " - YouTube";
        private bool _isHidden = false;
        public string Path = System.IO.Path.Combine(_folderPath, Consts.DefaultDirectoryName);
        public string HiddenPath = System.IO.Path.Combine(_folderPath, Consts.TemporaryDirectoryName);

        public string DefaultTrackPath { get; set; }
        public string DefaultTrackHiddenPath { get; set; }
        public string DefaultTrackName { get; set; }
        public string TmpTrackHiddenPath { get; set; }
        public string TmpTrackPath { get; set; }
        #endregion

        #region Ctor
        public FileHelper()
        {

        }
        #endregion

        #region Methods
        

        public static void EnsureDirectoryExist(string filePath)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
        }

        public void WriteToFile(string fileName, byte[] bytes, bool isHidden)
        {
            try
            {
                if (!isHidden)
                {
                    File.WriteAllBytes(Path + "\\" + fileName, bytes);
                }
                else
                {
                    File.WriteAllBytes(HiddenPath + "\\" + fileName, bytes);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
        }

        internal void RemoveFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException e)
            {

            }
        }

        public void RemoveContent(string path)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
            }
            catch (IOException e)
            {

            }
        }

        public void RenameFile(string oldNamePath, string newNamePath)
        {
            try
            {
                var newPath = CheckVideoFormat(newNamePath);
                if (newPath.Contains(_youtubeLastPartString))
                {
                    File.Move(oldNamePath, newPath.Replace(_youtubeLastPartString, string.Empty));
                }
                else
                {
                    File.Move(oldNamePath, newPath);
                }
            }
            catch (IOException e)
            {

            }
        }

        public static string GetApplicationFolder()
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetTempFileName()
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        }

        public static void OpenInExplorer(string path)
        {
            string cmd = "explorer.exe";
            string arg = "/select, " + path;
            Process.Start(cmd, arg);
        }

        public string CheckVideoFormat(string path)
        {
            if (path.Contains(".webm"))
            {
                return path.Replace(".webm", ".mp3")
                    .Replace(Consts.TemporaryDirectoryName, Consts.DefaultDirectoryName);
            }
            else if (path.Contains(".mp4"))
            {
                return path.Replace(".mp4", ".mp3")
                    .Replace(Consts.TemporaryDirectoryName, Consts.DefaultDirectoryName);
            }
            return string.Empty;
        }

        public string PrepareTrackForNotification(string trackName)
        {
            if (trackName.Contains(".mp3"))
            {
                return trackName.Replace(".mp3", string.Empty);
            }
            else if (trackName.Contains(".webm"))
            {
                return trackName.Replace(".webm", string.Empty);
            }
            else if (trackName.Contains(".mp4"))
            {
                return trackName.Replace(".mp4", string.Empty);
            }
            else if (trackName.Contains("- YouTube"))
            {
                return trackName.Replace("- YouTube", string.Empty);
            }
            return trackName;
        }
        #endregion
    }
}
