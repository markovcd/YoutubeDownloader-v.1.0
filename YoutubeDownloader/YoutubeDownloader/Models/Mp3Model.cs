
namespace YoutubeDownloader
{
    public enum Mp3ModelState
    {
        None, Downloading, Converting, Done, Error
    }

    sealed public class Mp3Model : BindableBase
    {
        public Mp3Model(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public Mp3Model(string path) : this(System.IO.Path.GetFileNameWithoutExtension(path), path) { }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
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
