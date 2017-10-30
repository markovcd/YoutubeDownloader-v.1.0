using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToastNotifications.Messages;
using VideoLibrary;

namespace YoutubeDownloader
{
    class Mp3ViewModel : BaseViewModel
    {
        #region Fields and Properties
        private ConnectionHelper _connectionHelper;
        private Converter _converter;
        private CursorControl _cursor;
        public event Action<string> OnffmpegStarted;
        public event Action<string> OnffmpegFinished;

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

        private void OnFFmpegStarted(string s1)
        {
            this.IsConvertingLabelVisible = Visibility.Visible;
            this.IsPercentLabelVisible = Visibility.Hidden;
            _cursor.Wait();
        }

        private void OnFFmpegFinished(string s1)
        {
            this.IsConvertingLabelVisible = Visibility.Hidden;
            _cursor.Arrow();
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            OnffmpegStarted += new Action<string>(OnFFmpegStarted);
            OnffmpegFinished += new Action<string>(OnFFmpegFinished);

            this._connectionHelper = new ConnectionHelper();
            this._converter = new Converter();
            this._cursor = new CursorControl();

            DefaultSetup();
        }

        private void DefaultSetup()
        {
            this.IsProgressDownloadVisible = Visibility.Hidden;
            this.IsPercentLabelVisible = Visibility.Hidden;
            this.IsConvertingLabelVisible = Visibility.Hidden;
            this.ConvertingLabelText = Consts.ConvertingPleaseWait;
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
                    IsPercentLabelVisible = Visibility.Visible;
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
                    ExtractAudioMp3FromVideo(fileHelper.HiddenPath + "\\" + tmpWOSpaces);
                    fileHelper.RemoveFile(tmpWOSpaces, true);
                    fileHelper.RenameFile(tmpOutputPathForAudioTrack, TrackNameManager.Instance.DefaultTrackPath);
                }
            }
            
            result = TrackNameManager.Instance.DefaultTrackName.Replace(".mp4", string.Empty) + "\nDownloaded";
            DefaultSetup();
            this.IsGoButtonEnabled = true;
        }

        public void ExtractAudioMp3FromVideo(string videoToWorkWith)
        {
            OnffmpegStarted("I've just started!");
            string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
            const string _temporaryFolderName = "YouTubeDownloaderTEMP";
            const string _defaultFolderName = "YouTubeDownloader";

            try
            {
                Process ffmpegProcess = new Process();
                var inputFile = videoToWorkWith;
                var tmp = videoToWorkWith.Replace(".mp4", ".mp3");
                var outputFile = tmp.Replace(_temporaryFolderName, _defaultFolderName);
                var mp3output = string.Empty;

                // TIP! Refer to https://trac.ffmpeg.org/wiki/Encode/MP3 for more infor about arguments you get use
                // or https://gist.github.com/protrolium/e0dbd4bb0f1a396fcb55 
                ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -codec:a libmp3lame -qscale:a 2 " + outputFile;
                ffmpegProcess.StartInfo.FileName = ffmpegExePath;
                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.RedirectStandardInput = true;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.CreateNoWindow = true;
                ffmpegProcess.Start();
                ffmpegProcess.BeginOutputReadLine();

                var tmpErrorOutput = ffmpegProcess.StandardError.ReadToEnd();
                Debug.WriteLine(tmpErrorOutput);
                ffmpegProcess.WaitForExit();
                OnffmpegFinished("I've just finished!");
                ffmpegProcess.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Occured: {0}", e);
            }
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
