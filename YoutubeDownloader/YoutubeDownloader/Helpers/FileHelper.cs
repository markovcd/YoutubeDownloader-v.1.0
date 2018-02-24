using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace YoutubeDownloader
{
    public static class FileHelper
    {

        #region Methods
        
        public static void EnsureDirectoryExist(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        public static string GetApplicationFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        public static void OpenInExplorer(string path)
        {
            const string cmd = "explorer.exe";
            const string arg = "/select, ";
            Process.Start(cmd, arg + path);
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public static string GetMp3FilePath(string videoFileName)
        {
            videoFileName = CleanFileName(videoFileName);

            return Path.Combine(SettingsSingleton.Instance.Model.Mp3DestinationDirectory,
                                Path.ChangeExtension(videoFileName, ".mp3"));
        }

        #endregion
    }
}
