using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeDownloader
{
    public sealed class FileHelper
    {
        #region Fields & Properties
        private static readonly string _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string _youtubeLastPartString = " - YouTube";
        public string Path = System.IO.Path.Combine(_folderPath, Consts.DefaultDirectoryName);
        public string HiddenPath = System.IO.Path.Combine(_folderPath, Consts.TemporaryDirectoryName);
        private bool _isHidden = false;
        #endregion

        #region Ctor
        public FileHelper()
        {

        }
        #endregion

        #region Methods
        public void CheckIfDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(Path))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(Path);
                }
                CreateHiddenFolder();
            }
            catch (IOException e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
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

        public void RemoveFile(string fileName, bool isHidden)
        {
            _isHidden = isHidden;
            try
            {
                if (CheckPossibleDuplicate(fileName))
                {
                    if (!isHidden)
                    {
                        File.Delete(Path + "\\" + fileName);
                    }
                    else
                    {
                        File.Delete(HiddenPath + "\\" + fileName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
            _isHidden = false;
        }

        public void RenameFile(string oldNamePath, string newNamePath)
        {
            try
            {
                if (newNamePath.Contains(_youtubeLastPartString))
                {
                    File.Move(oldNamePath, newNamePath.Replace(_youtubeLastPartString, string.Empty));
                }
                else
                {
                    File.Move(oldNamePath, newNamePath);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
        }

        private void CreateHiddenFolder()
        {
            try
            {
                if (!Directory.Exists(HiddenPath))
                {
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(HiddenPath);
                    directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception occured: {0}", e.ToString());
            }
        }

        public bool CheckPossibleDuplicate(string fileName)
        {
            if (!_isHidden)
            {
                var firstReplace = fileName.Replace(".mp4", ".mp3");
                var finalReplace = firstReplace.Replace(_youtubeLastPartString, string.Empty);
                return File.Exists(System.IO.Path.Combine(Path, finalReplace));
            }
            else
            {
                return File.Exists(System.IO.Path.Combine(HiddenPath, fileName));
            }
        }
        #endregion
    }
}
