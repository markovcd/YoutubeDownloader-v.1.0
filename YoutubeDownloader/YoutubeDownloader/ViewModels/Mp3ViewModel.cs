using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private Mp3Model model;
        private string _outputPath_TEMP;
        private string _trackName_TEMP;

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
                OnPropertyChanged("Mp3List");
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
        
        private double CurrentProgress;
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
        private async void GoButtonClicked()
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
            this.Mp3List = new ObservableCollection<Mp3Model>();
            this.model = new Mp3Model();

            DefaultSetup();
        }

        private void InitializeModel()
        {
            model = new Mp3Model()
            {
                Name = TrackNameManager.Instance.DefaultTrackName,
                CurrentProgress = CurrentProgress,
                IsProgressDownloadVisible = Visibility.Visible,
                IsPercentLabelVisible = Visibility.Visible,
                IsConvertingLabelVisible = Visibility.Hidden,
                IsOperationDoneLabelVisible = Visibility.Hidden,
                ConvertingLabelText = Consts.ConvertingPleaseWait
            };
        }

        private void DefaultSetup()
        {
            model.IsProgressDownloadVisible = Visibility.Hidden;
            model.IsPercentLabelVisible = Visibility.Hidden;
            model.IsConvertingLabelVisible = Visibility.Hidden;
            model.IsOperationDoneLabelVisible = Visibility.Visible;
            model.ConvertingLabelText = Consts.ConvertingPleaseWait;
            model.IsOperationDone = Consts.OperationDone;
            model.IsIndeterminate = false;

            this.CurrentProgress = 0;
            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
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
            model.IsConvertingLabelVisible = Visibility.Visible;
            model.IsPercentLabelVisible = Visibility.Hidden;
            model.IsIndeterminate = true;

            DispatchService.Invoke(() =>
            {
                shortToastMessage.ShowInformation("Converting...");
            });
        }

        private void AfterConversion()
        {
            DispatchService.Invoke(() =>
            {
                var message = TrackNameManager.Instance.DefaultTrackName.Replace(".mp4", string.Empty) + "\nDownloaded";
                if (message.Contains("- YouTube"))
                {
                    longToastMessage.ShowSuccess(message.Replace("- YouTube", string.Empty));
                }
                else
                {
                    longToastMessage.ShowSuccess(message);
                }
            });

            fileHelper.RemoveFile(_trackName_TEMP, true);
            fileHelper.RenameFile(_outputPath_TEMP, TrackNameManager.Instance.DefaultTrackPath);
            DefaultSetup();
        }

        private void SaveVideoToDisk()
        {
            using (var service = Client.For(YouTube.Default))
            {
                using (var video = service.GetVideo(YoutubeLinkUrl))
                {
                    var defaultTrackName = (fileHelper.Path + "\\" + video.FullName).Replace(".mp4", ".mp3");
                    TrackNameManager.Instance.DefaultTrackPath = defaultTrackName;
                    TrackNameManager.Instance.DefaultTrackName = video.FullName;
                    _trackName_TEMP = video.FullName.Replace(" ", string.Empty);

                    InitializeModel();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Mp3List.Add(model);
                    });

                    using (var outFile = File.OpenWrite(fileHelper.HiddenPath + "\\" + _trackName_TEMP))
                    {
                        using (var progressStream = new ProgressStream(outFile))
                        {
                            var streamLength = (long)video.StreamLength();

                            progressStream.BytesMoved += (sender, args) =>
                            {
                                model.CurrentProgress = args.StreamLength * 100 / streamLength;
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
