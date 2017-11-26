using System.ComponentModel;
using System.Windows;

namespace YoutubeDownloader
{
    sealed public class Mp3Model : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private double _currentProgress;
        public double CurrentProgress
        {
            get
            {
                return _currentProgress;
            }
            set
            {
                _currentProgress = value;
                OnPropertyChanged("CurrentProgress");
            }
        }

        private Visibility _isConvertingLabelVisible;
        public Visibility IsConvertingLabelVisible
        {
            get
            {
                return _isConvertingLabelVisible;
            }
            set
            {
                _isConvertingLabelVisible = value;
                OnPropertyChanged("IsConvertingLabelVisible");
            }
        }

        private Visibility _isPercentLabelVisible;
        public Visibility IsPercentLabelVisible
        {
            get
            {
                return _isPercentLabelVisible;
            }
            set
            {
                _isPercentLabelVisible = value;
                OnPropertyChanged("IsPercentLabelVisible");
            }
        }

        private Visibility _isProgressDownloadVisible;
        public Visibility IsProgressDownloadVisible
        {
            get
            {
                return _isProgressDownloadVisible;
            }
            set
            {
                _isProgressDownloadVisible = value;
                OnPropertyChanged("IsProgressDownloadVisible");
            }
        }

        private Visibility _isOperationDoneLabelVisible;
        public Visibility IsOperationDoneLabelVisible
        {
            get
            {
                return _isOperationDoneLabelVisible;
            }
            set
            {
                _isOperationDoneLabelVisible = value;
                OnPropertyChanged("IsOperationDoneLabelVisible");
            }
        }

        private string _isOperationDone;
        public string IsOperationDone
        {
            get
            {
                return _isOperationDone;
            }
            set
            {
                _isOperationDone = value;
                OnPropertyChanged("IsOperationDone");
            }
        }

        private string _convertingLabelText;
        public string ConvertingLabelText
        {
            get
            {
                return _convertingLabelText;
            }
            set
            {
                _convertingLabelText = value;
                OnPropertyChanged("ConvertingLabelText");
            }
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get
            {
                return _isIndeterminate;
            }
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
