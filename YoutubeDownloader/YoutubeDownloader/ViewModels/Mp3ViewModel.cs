using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ToastNotifications.Messages;
using VideoLibrary;

namespace YoutubeDownloader
{
    class Mp3ViewModel : BaseViewModel
    {
        #region Fields and Properties
        private ConnectionHelper _connectionHelper;

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
            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
            this._connectionHelper = new ConnectionHelper();
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
        private async Task SaveVideoToDiskAsync(string link)
        {
            await Task.Run(() =>
            {
                if (CheckIfInternetConnectivityIsOn())
                {
                    using (var service = Client.For(YouTube.Default))
                    {
                        using (var video = service.GetVideo(link))
                        {
                            using (var outFile = File.OpenWrite(fileHelper.Path + "\\" + video.FullName))
                            {
                                using (var progressStream = new ProgressStream(outFile))
                                {
                                    var streamLength = (long)video.StreamLength();

                                    progressStream.BytesMoved += (sender, args) =>
                                    {
                                        CurrentProgress = args.StreamLength * 100 / streamLength;
                                        Debug.WriteLine($"{CurrentProgress}% of video downloaded");
                                    };

                                    video.Stream().CopyTo(progressStream);
                                }
                            }
                        }
                    }
                }
            });
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
