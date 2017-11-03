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
        private string _outputPath_TEMP;
        private string _trackName_TEMP;

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
                        BackgroundMainTask();
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
            this.IsGoButtonEnabled = true;
            this.IsIndeterminate = false;
            this.CurrentProgress = 0;

            this._outputPath_TEMP = string.Empty;
            this._trackName_TEMP = string.Empty;

            TrackNameManager.Instance.DefaultTrackPath = string.Empty;
            TrackNameManager.Instance.DefaultTrackName = string.Empty;
        }

        private void BackgroundMainTask()
        {
            CancellationToken cancellationToken = new CancellationToken();
            Task.Factory.StartNew(() =>
            {
                SaveVideoToDisk();
            }, cancellationToken);
        }

        private void BeforeConversion()
        {
            this.IsConvertingLabelVisible = Visibility.Visible;
            this.IsPercentLabelVisible = Visibility.Hidden;
            this.IsIndeterminate = true;

            DispatchService.Invoke(() =>
            {
                shortToastMessage.ShowInformation("Converting...");
            });
        }

        private void AfterConversion()
        {
            DispatchService.Invoke(() =>
            {
                longToastMessage.ShowSuccess(TrackNameManager.Instance.DefaultTrackName.Replace(".mp4", string.Empty) + "\nDownloaded");
            });

            fileHelper.RemoveFile(_trackName_TEMP, true);
            fileHelper.RenameFile(_outputPath_TEMP, TrackNameManager.Instance.DefaultTrackPath);
            DefaultSetup();
        }

        private void SaveVideoToDisk()
        {
            this.IsGoButtonEnabled = false;
            using (var service = Client.For(YouTube.Default))
            {
                using (var video = service.GetVideo(YoutubeLinkUrl))
                {
                    var defaultTrackName = (fileHelper.Path + "\\" + video.FullName).Replace(".mp4", ".mp3");
                    TrackNameManager.Instance.DefaultTrackPath = defaultTrackName;
                    TrackNameManager.Instance.DefaultTrackName = video.FullName;
                    _trackName_TEMP = video.FullName.Replace(" ", string.Empty);
                    IsProgressDownloadVisible = Visibility.Visible;
                    IsPercentLabelVisible = Visibility.Visible;
                    using (var outFile = File.OpenWrite(fileHelper.HiddenPath + "\\" + _trackName_TEMP))
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

                    _outputPath_TEMP = (fileHelper.Path + "\\" + _trackName_TEMP).Replace(".mp4", ".mp3");
                    ExtractAudioFromVideo(fileHelper.HiddenPath + "\\" + _trackName_TEMP);
                }
            }
        }

        public void ExtractAudioFromVideo(string videoToWorkWith)
        {
            string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");

            try
            {
                BeforeConversion();
                Process ffmpegProcess = new Process();
                var inputFile = videoToWorkWith;
                var tmp = videoToWorkWith.Replace(".mp4", ".mp3");
                var outputFile = tmp.Replace(Consts.TemporaryDirectoryName, Consts.DefaultDirectoryName);
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
                ffmpegProcess.BeginErrorReadLine();

                ffmpegProcess.EnableRaisingEvents = true;
                ffmpegProcess.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);
                ffmpegProcess.Exited += new EventHandler(OnConversionExited);

                var tmpErrorOutput = ffmpegProcess.StandardError.ReadToEnd();
                Debug.WriteLine(tmpErrorOutput);
                ffmpegProcess.WaitForExit();
                ffmpegProcess.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Occured: {0}", e);
            }
        }

        private void OnConversionExited(object sender, EventArgs e)
        {
            AfterConversion();
        }

        private int currentLine = 0;
        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("Input line: {0} ({1:m:s:fff})", currentLine++, DateTime.Now);
        }

        private bool CheckIfFileAlreadyExists(string FileName)
        {
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(FileName);

            if (fileHelper.CheckPossibleDuplicate(video.FullName))
            {
                shortToastMessage.ShowInformation(Consts.FileAlreadyExistsInfo);
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
                    shortToastMessage.ShowError(Consts.InternetConnectionError);
                }
            }
            return false;
        }

        private bool ValidateEditFieldString()
        {
            if (YoutubeLinkUrl == string.Empty)
            {
                shortToastMessage.ShowWarning(Consts.LinkValidatorEmpty);
                return false;
            }
            else if (!YoutubeLinkUrl.Contains(Consts.LinkPartValidation))
            {
                shortToastMessage.ShowWarning(Consts.LinkValidatorIsNotValid);
                YoutubeLinkUrl = string.Empty;
                return false;
            }
            return true;
        }
        #endregion
    }
}
