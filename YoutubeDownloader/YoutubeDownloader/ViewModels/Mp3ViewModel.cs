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
        private FileHelper _fileHelper;
        private Process _process;
        private int _currentLine;

        private ObservableCollection<Mp3Model> _mp3List;
        public ObservableCollection<Mp3Model> Mp3List
        {
            get { return _mp3List; }
            set { SetProperty(ref _mp3List, value); }
        }

        private ObservableCollection<QualityModel> _qualityList;
        public ObservableCollection<QualityModel> QualityList
        {
            get { return _qualityList; }
            set { SetProperty(ref _qualityList, value); }
        }

        private QualityModel _qualityModel;
        public QualityModel QualityModel
        {
            get { return _qualityModel; }
            set { SetProperty(ref _qualityModel, value); }
        }

        private string _youtubeLinkUrl;
        public string YoutubeLinkUrl
        {
            get { return _youtubeLinkUrl; }
            set { SetProperty(ref _youtubeLinkUrl, value); }
        }

        private bool _isFocused;
        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                SetProperty(ref _isFocused, value);

                if (_isFocused)
                {
                    YoutubeLinkUrl = string.Empty;
                }
                else if (!_isFocused && YoutubeLinkUrl == string.Empty)
                {
                    YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
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

        #region Constructor
        public Mp3ViewModel()
        {
            Initialize();
            InitializeQualityCollection();
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
                        SaveVideoToDisk();
                    }
                }
            }
        }
        #endregion

        #region Methods Private
        private void Initialize()
        {
            this._connectionHelper = new ConnectionHelper();
            this._converter = new Converter();
            this._cursor = new CursorControl();
            this._mp3List = new ObservableCollection<Mp3Model>();
            this._fileHelper = new FileHelper();

            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
        }

        private void InitializeQualityCollection()
        {
            QualityList = new ObservableCollection<QualityModel>
            {
                new QualityModel() { Quality = "128k" },
                new QualityModel() { Quality = "192k" },
                new QualityModel() { Quality = "256k" },
                new QualityModel() { Quality = "320k" },
            };

            QualityModel = QualityList[3];
        }

        private void SaveVideoToDisk()
        {
            Task.Factory.StartNew(() =>
            {
                var CurrentFile = new FileHelper();
                var Mp3Model = new Mp3Model();

                using (var service = Client.For(YouTube.Default))
                {
                    using (var video = service.GetVideo(YoutubeLinkUrl))
                    {
                        CurrentFile.DefaultTrackName = video.FullName;
                        CurrentFile.DefaultTrackPath = CurrentFile.Path + "\\" + CurrentFile.DefaultTrackName;
                        CurrentFile.DefaultTrackHiddenPath = CurrentFile.HiddenPath + "\\" + CurrentFile.DefaultTrackName;
                        CurrentFile.TmpTrackPath = CurrentFile.PreparePathForFFmpeg(CurrentFile.DefaultTrackHiddenPath);

                        Mp3Model = new Mp3Model()
                        {
                            Name = CurrentFile.CheckVideoFormat(video.FullName),
                            IsProgressDownloadVisible = Visibility.Visible,
                            IsPercentLabelVisible = Visibility.Visible,
                            IsConvertingLabelVisible = Visibility.Hidden,
                            IsOperationDoneLabelVisible = Visibility.Hidden,
                            ConvertingLabelText = Consts.ConvertingPleaseWait,
                            CurrentProgress = 0,
                        };

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this._mp3List.Add(Mp3Model);
                        }));

                        using (var outFile = File.OpenWrite(CurrentFile.TmpTrackPath))
                        {
                            using (var progressStream = new ProgressStream(outFile))
                            {
                                var streamLength = (long)video.StreamLength();

                                progressStream.BytesMoved += (sender, args) =>
                                {
                                    Mp3Model.CurrentProgress = args.StreamLength * 100 / streamLength;
                                    Debug.WriteLine($"{Mp3Model.CurrentProgress}% of video downloaded");
                                };

                                video.Stream().CopyTo(progressStream);
                            }
                        }
                        BeforeConversion(Mp3Model);
                        ExtractAudioFromVideo(CurrentFile);
                        AfterConversion(Mp3Model, CurrentFile);
                    }
                }
            });
        }

        private void ExtractAudioFromVideo(FileHelper fileHelper)
        {
            var videoToWorkWith = fileHelper.TmpTrackPath;
            var ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
            var output = fileHelper.CheckVideoFormat(videoToWorkWith);
            var standardErrorOutput = string.Empty;
            var quality = QualityModel.Quality;

            fileHelper.DefaultTrackHiddenPath = videoToWorkWith;
            fileHelper.TmpTrackPath = output;

            try
            {
                _process = new Process();
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardInput = true;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                _process.StartInfo.FileName = ffmpegExePath;
                _process.StartInfo.Arguments = " -i " + videoToWorkWith + " -codec:a libmp3lame -b:a " + quality + " " + output;
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
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: implement logger
            Debug.WriteLine("Input line: {0} ({1:m:s:fff})", _currentLine++, DateTime.Now);
        }

        private void BeforeConversion(Mp3Model model)
        {
            model.IsConvertingLabelVisible = Visibility.Visible;
            model.IsPercentLabelVisible = Visibility.Hidden;
            model.IsIndeterminate = true;

            DispatchService.Invoke(() =>
            {
                shortToastMessage.ShowInformation("Converting...");
            });
        }

        private void AfterConversion(Mp3Model model, FileHelper fileHelper)
        {
            DispatchService.Invoke(() =>
            {
                longToastMessage.ShowSuccess(fileHelper.PrepareTrackForNotification(fileHelper.DefaultTrackName));
            });

            fileHelper.RenameFile(fileHelper.TmpTrackPath, fileHelper.DefaultTrackPath);
            fileHelper.RemoveFile(fileHelper.DefaultTrackHiddenPath);

            model.IsProgressDownloadVisible = Visibility.Hidden;
            model.IsPercentLabelVisible = Visibility.Hidden;
            model.IsConvertingLabelVisible = Visibility.Hidden;
            model.IsOperationDoneLabelVisible = Visibility.Visible;
            model.ConvertingLabelText = Consts.ConvertingPleaseWait;
            model.IsOperationDone = Consts.OperationDone;
            model.IsIndeterminate = false;
        }
        #endregion

        #region Validators
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
                YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
                return false;
            }
            return true;
        }
        #endregion
    }
}
