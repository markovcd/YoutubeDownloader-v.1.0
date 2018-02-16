using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToastNotifications.Messages;

namespace YoutubeDownloader
{
    class Mp3ViewModel : BaseViewModel
    {
        #region Fields and Properties
        private ConnectionHelper _connectionHelper;
        private CursorControl _cursor;


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
        public ICommand StartMp3DownloadCommand { get { return new RelayCommand<string>(StartMp3Download); } }

        public ICommand OpenMp3LocationCommand { get { return new RelayCommand<Mp3Model>(OpenMp3Location); } }
        #endregion

        #region Constructor
        public Mp3ViewModel()
        {
            Initialize();
            InitializeQualityCollection();
        }
        #endregion

        #region Events
        private void StartMp3Download(string youtubeLinkUrl)
        {
            if (ValidateEditFieldString(youtubeLinkUrl) && CheckIfInternetConnectivityIsOn())
            {
                SaveVideoToDisk(youtubeLinkUrl);   
            }
        }

        private void OpenMp3Location(Mp3Model mp3Model)
        {
            FileHelper.OpenInExplorer(mp3Model.Path);

        }
        #endregion

        #region Methods Private
        private void Initialize()
        {
            _connectionHelper = new ConnectionHelper();
            _cursor = new CursorControl();
            _mp3List = new ObservableCollection<Mp3Model>();

            YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
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

        private void SaveVideoToDisk(string youtubeLinkUrl)
        {
            Task.Factory.StartNew(() =>
            {

                var tempPath = FileHelper.GetTempFileName();

                Mp3Model mp3Model;

                using (var outFile = File.OpenWrite(tempPath))
                {
                    using (var videoDownloader = new VideoDownloader(youtubeLinkUrl, outFile))
                    {
                        var destPath = FileHelper.GetMp3FilePath(videoDownloader.CurrentVideo.FullName);

                        mp3Model = new Mp3Model
                        {
                            Name = Path.GetFileNameWithoutExtension(destPath),
                            Path = destPath,
                            Url = youtubeLinkUrl,
                            State = Mp3ModelState.Downloading,
                        };

                        if (File.Exists(mp3Model.Path))
                        {
                            shortToastMessage.ShowInformation(Consts.FileAlreadyExistsInfo);
                            return;
                        }

                        FileHelper.EnsureDirectoryExist(mp3Model.Path);

                        Application.Current.Dispatcher.BeginInvoke(new Action(() => _mp3List.Add(mp3Model)));

                        videoDownloader.ProgressChanged += (s, a) =>
                        {
                            mp3Model.CurrentProgress = a.CurrentProgress;
                            Debug.WriteLine($"{a.CurrentProgress}% of video downloaded");
                        };

                        videoDownloader.Download();
                    }
                }


                mp3Model.State = Mp3ModelState.Converting;             
                DispatchService.Invoke(() => shortToastMessage.ShowInformation("Converting..."));

                var converter = new Converter();
                
                converter.ProgressChanged += (s, a) =>  mp3Model.CurrentProgress = a.CurrentProgress;
                

                converter.ExtractAudioMp3FromVideo(tempPath, mp3Model.Path, QualityModel.Quality);

                File.Delete(tempPath);

                DispatchService.Invoke(() =>
                {
                    longToastMessage.ShowSuccess(mp3Model.Name);
                });

                mp3Model.State = Mp3ModelState.Done;

            });
        }


        #endregion

        #region Validators

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

        private bool ValidateEditFieldString(string youtubeLinkUrl)
        {
            var urlParser = new YoutubeUrlParser { Url = youtubeLinkUrl };
            

            if (urlParser.UrlType == YoutubeUrlType.Empty)
            {
                shortToastMessage.ShowWarning(Consts.LinkValidatorEmpty);
                return false;
            }
            else if (urlParser.UrlType == YoutubeUrlType.Playlist || urlParser.UrlType == YoutubeUrlType.Error)
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
