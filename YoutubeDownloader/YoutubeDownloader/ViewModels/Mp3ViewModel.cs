using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ToastNotifications.Messages;
using VideoLibrary;
using System.Threading;

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

        private bool _isGoButtonEnabled = true;
        public bool IsGoButtonEnabled
        {
            get
            {
                return _isGoButtonEnabled;
            }
            set
            {
                _isGoButtonEnabled = value;
                OnPropertyChanged("IsGoButtonEnabled");
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
                return new RelayCommand(GoButtonClicked, CanExecute);
            }
        }
        #endregion

        #region Ctor
        public Mp3ViewModel()
        {
            Initialize();
        }
        #endregion

        #region Events
        private void GoButtonClicked()
        {
            if (ValidateEditFieldString())
            {
                if (!CheckIfFileAlreadyExists(YoutubeLinkUrl))
                {
                    if (CheckIfInternetConnectivityIsOn())
                    {
                        TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                        string result = string.Empty;
                        CancellationToken cancellationToken = new CancellationToken();
                        Task.Factory.StartNew(() => SaveVideoToDisk(out result)).
                            ContinueWith(w =>
                            {
                                longToastMessage.ShowSuccess(result);
                            },
                            cancellationToken,
                            TaskContinuationOptions.None, scheduler);
                    }
                }
            }
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            this._connectionHelper = new ConnectionHelper();
            this._converter = new Converter();

            DefaultSetup();
        }

        private void DefaultSetup()
        {
            this.IsProgressDownloadVisible = Visibility.Hidden;
            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
            this.CurrentProgress = 0;

            TrackNameManager.Instance.DefaultTrackPath = string.Empty;
            TrackNameManager.Instance.DefaultTrackName = string.Empty;
        }

        private void SaveVideoToDisk(out string result)
        {
            this.IsGoButtonEnabled = false;
            using (var service = Client.For(YouTube.Default))
            {
                using (var video = service.GetVideo(YoutubeLinkUrl))
                {
                    var defaultTrackName = (fileHelper.Path + "\\" + video.FullName).Replace(".mp4", ".mp3");
                    TrackNameManager.Instance.DefaultTrackPath = defaultTrackName;
                    TrackNameManager.Instance.DefaultTrackName = video.FullName;
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
                    fileHelper.RenameFile(tmpOutputPathForAudioTrack, TrackNameManager.Instance.DefaultTrackPath);
                }
            }
            
            result = TrackNameManager.Instance.DefaultTrackName.Replace(".mp4", string.Empty) + "\nDownloaded";
            DefaultSetup();
            this.IsGoButtonEnabled = true;
        }

        private bool CheckIfFileAlreadyExists(string FileName)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(FileName);

            if (fileHelper.CheckPossibleDuplicate(video.FullName))
            {
                notifier.ShowInformation(Consts.FileAlreadyExistsInfo);
                return true;
            }
            return false;
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
            if (YoutubeLinkUrl == string.Empty)
            {
                notifier.ShowWarning(Consts.LinkValidatorEmpty);
                return false;
            }
            else if (!YoutubeLinkUrl.Contains(Consts.LinkPartValidation))
            {
                notifier.ShowWarning(Consts.LinkValidatorIsNotValid);
                YoutubeLinkUrl = string.Empty;
                return false;
            }
            return true;
        }
        #endregion
    }
}
