using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace YoutubeDownloader
{
    public sealed class FileHelper
    {

        #region Methods
        
        public static string RemoveYoutubeSuffix(string fileName)
        {
            const string youtubeSuffix = " - YouTube";

            var ext = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);

            if (!name.EndsWith(youtubeSuffix)) return fileName;

            return name.Substring(0, name.Length - youtubeSuffix.Length) + ext;
        }


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

        public static string GetMp3FilePath(string videoFileName, bool removeYoutubeSuffix = true)
        {
            if (removeYoutubeSuffix) videoFileName = RemoveYoutubeSuffix(videoFileName);

            return Path.Combine(SettingsSingleton.Instance.Model.Mp3DestinationDirectory,
                                Path.ChangeExtension(videoFileName, ".mp3"));
        }

        #endregion
    }
}
