
namespace YoutubeDownloader
{
    public enum Mp3ModelState
    {
        None, Downloading, Downloaded, Converting, Done, Error
    }

    sealed public class Mp3Model : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private string _url;
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private Mp3ModelState _state;
        public Mp3ModelState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        private double _currentProgress;
        public double CurrentProgress
        {
            get => _currentProgress;
            set => SetProperty(ref _currentProgress, value);
        }

        private string _quality;
        public string Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

    }
}
