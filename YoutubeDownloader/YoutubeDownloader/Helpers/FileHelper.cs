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
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
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

        public static string GetMp3FilePath(string videoFileName, bool removeYoutubeSuffix = true)
        {
            if (removeYoutubeSuffix) videoFileName = RemoveYoutubeSuffix(videoFileName);

            return Path.Combine(SettingsSingleton.Instance.Model.Mp3DestinationDirectory,
                                Path.ChangeExtension(videoFileName, ".mp3"));
        }

        #endregion
    }
}
