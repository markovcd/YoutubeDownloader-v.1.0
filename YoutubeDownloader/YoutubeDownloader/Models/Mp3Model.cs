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

        private Visibility _isConvertingLabelVisible;
        public Visibility IsConvertingLabelVisible
        {
            get { return _isConvertingLabelVisible; }
            set { SetProperty(ref _isConvertingLabelVisible, value); }
        }


        private Visibility _isPercentLabelVisible;
        public Visibility IsPercentLabelVisible
        {
            get { return _isPercentLabelVisible; }
            set { SetProperty(ref _isPercentLabelVisible, value); }
        }

        private Visibility _isProgressDownloadVisible;
        public Visibility IsProgressDownloadVisible
        {
            get { return _isProgressDownloadVisible; }
            set { SetProperty(ref _isProgressDownloadVisible, value); }
        }

        private Visibility _isOperationDoneLabelVisible;
        public Visibility IsOperationDoneLabelVisible
        {
            get { return _isOperationDoneLabelVisible; }
            set { SetProperty(ref _isOperationDoneLabelVisible, value); }
        }

        private string _isOperationDone;
        public string IsOperationDone
        {
            get { return _isOperationDone; }
            set { SetProperty(ref _isOperationDone, value); }
        }

        private string _convertingLabelText;
        public string ConvertingLabelText
        {
            get { return _convertingLabelText; }
            set { SetProperty(ref _convertingLabelText, value); }
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set { SetProperty(ref _isIndeterminate, value); }
        }

        private bool CanExecute()
        {
            return true;
        }
    }
}
