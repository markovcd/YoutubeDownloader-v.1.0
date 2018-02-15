using System.ComponentModel;
using System.Windows;

namespace YoutubeDownloader
{
    public enum Mp3ModelState
    {
        None, Downloading, Converting, Done, Error
    }

    sealed public class Mp3Model : BindableBase
    {
        public string Name { get; set; }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        public string FileName
        {
            get { return System.IO.Path.GetFileName(Path); }
        }

        public string FileNameWithoutExtension
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(Path); }
        }

        private Mp3ModelState _state;
        public Mp3ModelState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        private double _currentProgress;
        public double CurrentProgress
        {
            get { return _currentProgress; }
            set { SetProperty(ref _currentProgress, value); }
        }
    }
}
