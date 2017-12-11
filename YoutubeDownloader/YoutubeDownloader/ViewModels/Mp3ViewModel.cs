using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        private CursorControl _cursor;
        private Converter _converter;
        private Mp3Model _model;
        private Process _process;

        private string _defaultTrackPath;
        private string _defaultTrackHiddenPath;
        private string _defaultTrackName;
        private string _tmpTrackPath;
        private string _tmpTrackHiddenPath;
        private double _currentProgress;
        private int _currentLine;

        private ObservableCollection<Mp3Model> _mp3List;
        public ObservableCollection<Mp3Model> Mp3List
        {
            get
            {
                return _mp3List;
            }
            set
            {
                _mp3List = value;
                OnPropertyChanged(nameof(Mp3List));
            }
        }

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
                OnPropertyChanged(nameof(YoutubeLinkUrl));
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
            Task.Factory.StartNew(() =>
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
            });
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            this._connectionHelper = new ConnectionHelper();
            this._converter = new Converter();
            this._cursor = new CursorControl();
            this._model = new Mp3Model();
            this._mp3List = new ObservableCollection<Mp3Model>();

            DefaultSetup();
        }

        private void InitializeModel()
        {
            _model = new Mp3Model()
            {
                Name = _defaultTrackName,
                CurrentProgress = _currentProgress,
                IsProgressDownloadVisible = Visibility.Visible,
                IsPercentLabelVisible = Visibility.Visible,
                IsConvertingLabelVisible = Visibility.Hidden,
                IsOperationDoneLabelVisible = Visibility.Hidden,
                ConvertingLabelText = Consts.ConvertingPleaseWait
            };
        }

        private void DefaultSetup()
        {
            _model.IsProgressDownloadVisible = Visibility.Hidden;
            _model.IsPercentLabelVisible = Visibility.Hidden;
            _model.IsConvertingLabelVisible = Visibility.Hidden;
            _model.IsOperationDoneLabelVisible = Visibility.Visible;
            _model.ConvertingLabelText = Consts.ConvertingPleaseWait;
            _model.IsOperationDone = Consts.OperationDone;
            _model.IsIndeterminate = false;

            this._currentProgress = 0;
            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
            this._defaultTrackPath = string.Empty;
            this._defaultTrackName = string.Empty;
        }

        private void BackgroundMainTask()
        {
            SaveVideoToDisk();
        }

        private void BeforeConversion()
        {
            _model.IsConvertingLabelVisible = Visibility.Visible;
            _model.IsPercentLabelVisible = Visibility.Hidden;
            _model.IsIndeterminate = true;

            DispatchService.Invoke(() =>
            {
                shortToastMessage.ShowInformation("Converting...");
            });
        }

        private void AfterConversion()
        {
            DispatchService.Invoke(() =>
            {
                longToastMessage.ShowSuccess(fileHelper.PrepareTrackForNotification(_defaultTrackName));
            });
            
            fileHelper.RenameFile(_tmpTrackPath, _defaultTrackPath);
            fileHelper.RemoveContent(fileHelper.HiddenPath);

            DefaultSetup();
        }

        private void SaveVideoToDisk()
        {
            using (var service = Client.For(YouTube.Default))
            {
                using (var video = service.GetVideo(YoutubeLinkUrl))
                {
                    _defaultTrackName = video.FullName;
                    _defaultTrackPath = fileHelper.Path + "\\" + _defaultTrackName;
                    _defaultTrackHiddenPath = fileHelper.HiddenPath + "\\" + _defaultTrackName;
                    _tmpTrackPath = fileHelper.PreparePathForFFmpeg(_defaultTrackHiddenPath);

                    InitializeModel();

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this._mp3List.Add(_model);
                    }));

                    using (var outFile = File.OpenWrite(_tmpTrackPath))
                    {
                        using (var progressStream = new ProgressStream(outFile))
                        {
                            var streamLength = (long)video.StreamLength();

                            progressStream.BytesMoved += (sender, args) =>
                            {
                                _model.CurrentProgress = args.StreamLength * 100 / streamLength;
                                Debug.WriteLine($"{_model.CurrentProgress}% of video downloaded");
                            };

                            video.Stream().CopyTo(progressStream);
                        }
                    }
                    ExtractAudioFromVideo(_tmpTrackPath);
                }
            }
        }

        public void ExtractAudioFromVideo(string videoToWorkWith)
        {
            var ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
            var output = fileHelper.CheckVideoFormat(videoToWorkWith);
            var standardErrorOutput = string.Empty;
            _tmpTrackPath = output;

            try
            {
                BeforeConversion();
                _process = new Process();
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardInput = true;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _process.StartInfo.FileName = ffmpegExePath;
                _process.StartInfo.Arguments = " -i " + videoToWorkWith + " -vn -f mp3 -ab 192k " + output;
                _process.Start();
                _process.EnableRaisingEvents = true;
                _process.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);
                _process.Exited += new EventHandler(OnConversionExited);
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _process.WaitForExit();
                _process.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Occured: {0}", e);
            }
        }

        private void OnConversionExited(object sender, EventArgs e)
        {
            _process.ErrorDataReceived -= OnErrorDataReceived;
            _process.Exited -= OnConversionExited;

            AfterConversion();
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: implement logger
            Debug.WriteLine("Input line: {0} ({1:m:s:fff})", _currentLine++, DateTime.Now);
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
