
namespace YoutubeDownloader
{
    public enum Mp3ModelState
    {
        None, Downloading, Converting, Done, Error
    }

    sealed public class Mp3Model : BindableBase
    {
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

        private string _url;
        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
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
