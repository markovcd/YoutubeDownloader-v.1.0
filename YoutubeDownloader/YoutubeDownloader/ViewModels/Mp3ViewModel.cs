using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ToastNotifications.Messages;
using VideoLibrary;

namespace YoutubeDownloader
{
    class Mp3ViewModel : BaseViewModel
    {
        #region Fields and Properties
        private ConnectionHelper _connectionHelper;
        private Converter _converter;

        private string _youtubeLinkUrl;
        public string YoutubeLinkUrl
        {
            get
            {
                return _youtubeLinkUrl;
            }
            set
            {
                _youtubeLinkUrl = value;
                OnPropertyChanged("YoutubeLinkUrl");
            }
        }

        private bool _isFocused;
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                _isFocused = value;
                OnPropertyChanged("IsYouTubeTextBoxFocused");
                if (_isFocused)
                {
                    YoutubeLinkUrl = string.Empty;
                }
            }
        }

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
        #endregion

        #region Commands
        public ICommand GoButtonCommand
        {
            get
            {
                return new RelayCommand(GoButtonClickedAsync, CanExecute);
            }
        }
        #endregion

        #region Ctor
        public Mp3ViewModel()
        {
            DefaultValues();
        }
        #endregion

        #region Events
        private async void GoButtonClickedAsync()
        {
            if (ValidateEditFieldString())
            {
                await SaveVideoToDiskAsync(YoutubeLinkUrl);
            }
        }
        #endregion

        #region Methods
        private void DefaultValues()
        {
            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
            this.IsProgressDownloadVisible = Visibility.Hidden;
            this._connectionHelper = new ConnectionHelper();
            this._converter = new Converter();
        }

        private async Task SaveVideoToDiskAsync(string link)
        {
            if (!CheckIfFileAlreadyExists(link))
            {
                await Task.Run(() =>
                {
                    if (CheckIfInternetConnectivityIsOn())
                    {
                        using (var service = Client.For(YouTube.Default))
                        {
                            using (var video = service.GetVideo(link))
                            {
                                var defaultTrackName = (fileHelper.Path + "\\" + video.FullName).Replace(".mp4", ".mp3");
                                TrackNameManager.Instance.DefaultTrackName = defaultTrackName;
                                var tmpWOSpaces = video.FullName.Replace(" ", string.Empty);
                                IsProgressDownloadVisible = Visibility.Visible;
                                using (var outFile = File.OpenWrite(fileHelper.HiddenPath + "\\" + tmpWOSpaces))
                                {
                                    using (var progressStream = new ProgressStream(outFile))
                                    {
                                        var streamLength = (long)video.StreamLength();

                                        progressStream.BytesMoved += (sender, args) =>
                                        {
                                            CurrentProgress = args.StreamLength * 100 / streamLength;
                                            // TODO: Remove Debug.Writeline() in final version
                                            Debug.WriteLine($"{CurrentProgress}% of video downloaded");
                                        };

                                        video.Stream().CopyTo(progressStream);
                                    }
                                }

                                var tmpOutputPathForAudioTrack = (fileHelper.Path + "\\" + tmpWOSpaces).Replace(".mp4", ".mp3");
                                _converter.ExtractAudioMp3FromVideo(fileHelper.HiddenPath + "\\" + tmpWOSpaces);
                                fileHelper.RemoveFile(tmpWOSpaces, true);
                                fileHelper.RenameFile(tmpOutputPathForAudioTrack, TrackNameManager.Instance.DefaultTrackName);
                                TrackNameManager.Instance.DefaultTrackName = string.Empty;
                            }
                        }
                    }
                    IsProgressDownloadVisible = Visibility.Hidden;
                    YoutubeLinkUrl = string.Empty;
                });
            }
            else
            {
                notifier.ShowInformation(Consts.FileAlreadyExistsInfo);
            }
        }

        private void TrimAudioTrack()
        {
            
        }

        private bool CheckIfFileAlreadyExists(string FileName)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(FileName);
            return fileHelper.CheckPossibleDuplicate(video.FullName);
        }

        private bool CheckIfInternetConnectivityIsOn()
        {
            if (_connectionHelper != null)
            {
                if (_connectionHelper.CheckForInternetConnection())
                {
                    return true;
                }
                else
                {
                    notifier.ShowError(Consts.InternetConnectionError);
                }
            }
            return false;
        }

        private bool ValidateEditFieldString()
        {
            if (YoutubeLinkUrl != string.Empty)
            {
                if (YoutubeLinkUrl.Contains(Consts.LinkPartValidation))
                {
                    return true;
                }
                else
                {
                    notifier.ShowWarning(Consts.LinkValidatorIsNotValid);
                    YoutubeLinkUrl = string.Empty;
                    return false;
                }
            }
            else
            {
                notifier.ShowWarning(Consts.LinkValidatorEmpty);
                return false;
            }
        }
        #endregion
    }
}
