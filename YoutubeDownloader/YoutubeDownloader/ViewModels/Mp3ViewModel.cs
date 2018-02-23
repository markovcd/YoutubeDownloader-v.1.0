using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Threading;

namespace YoutubeDownloader
{
    class Mp3ViewModel : BaseViewModel
    {
        #region Fields and Properties
        private ConnectionHelper _connectionHelper;
        private CursorControl _cursor;
        private Task _downloadTask;
        private Task _convertTask;
        private CancellationToken _downloadCancellation;
        private CancellationToken _convertCancellation;

        private ObservableCollection<Mp3Model> _mp3List;
        public ObservableCollection<Mp3Model> Mp3List
        {
            get => _mp3List;
            set => SetProperty(ref _mp3List, value);
        }

        private ObservableCollection<QualityModel> _qualityList;
        public ObservableCollection<QualityModel> QualityList
        {
            get => _qualityList;
            set => SetProperty(ref _qualityList, value);
        }

        private QualityModel _qualityModel;
        public QualityModel QualityModel
        {
            get => _qualityModel;
            set => SetProperty(ref _qualityModel, value);
        }

        private YoutubeUrl _youtubeUrl;
        public YoutubeUrl YoutubeUrl
        {
            get => _youtubeUrl;
            set => SetProperty(ref _youtubeUrl, value);
        }

        private bool _downloadPlaylist;
        public bool DownloadPlaylist
        {
            get => _downloadPlaylist;
            set => SetProperty(ref _downloadPlaylist, value);
        }

        #endregion

        #region Commands
        public ICommand StartMp3DownloadCommand => new RelayCommand(StartMp3Download, CanStartMp3Download);
        public ICommand OpenMp3LocationCommand => new RelayCommand<Mp3Model>(OpenMp3Location, CanOpenMp3Location);

        #endregion

        #region Constructor
        public Mp3ViewModel()
        {
            Initialize();
            InitializeQualityCollection();
            InitializeValidationMappings();
        }
        #endregion

        #region Events
        private void StartMp3Download()
        {
            YoutubeUrl = new YoutubeUrl(); // clears youtube url textbox

            Task.Run(() => 
            {
                if (!CheckIfInternetConnectivityIsOn()) return;

                if (YoutubeUrl.UrlType == YoutubeUrlType.Video)
                {
                    AddMp3Models(YoutubeUrl.Url).ToArray();
                }
                else if (!DownloadPlaylist && YoutubeUrl.UrlType == YoutubeUrlType.VideoAndPlaylist)
                {
                    AddMp3Models(YoutubeUrl.Url).ToArray();
                }
                else if (YoutubeUrl.UrlType == YoutubeUrlType.Playlist)
                {
                    AddMp3ModelsFromPlaylist(YoutubeUrl.PlaylistId).ToArray();
                }
                else if (DownloadPlaylist && YoutubeUrl.UrlType == YoutubeUrlType.VideoAndPlaylist)
                {
                    AddMp3ModelsFromPlaylist(YoutubeUrl.PlaylistId).ToArray();
                }
            });
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
            _downloadTask = KeepDownloadingVideos();
            _convertTask = KeepConvertingVideos();
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

        private void InitializeValidationMappings()
        {
            AddValidationMapping(nameof(YoutubeUrl), ValidateYoutubeUrl);
        }

        private Task KeepDownloadingVideos()
        {
            _downloadCancellation = new CancellationToken();

            return Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    var mp3Model = _mp3List.FirstOrDefault(mp3 => mp3.State == Mp3ModelState.None);
                    if (mp3Model == null) continue;
                    DownloadYoutubeVideo(mp3Model);
                }
            }, _downloadCancellation);
        }

        private Task KeepConvertingVideos()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    var mp3Model = _mp3List.FirstOrDefault(mp3 => mp3.State == Mp3ModelState.Downloaded);
                    if (mp3Model == null) continue;
                    ConvertYoutubeVideo(mp3Model);
                }
            });
        }

        private IEnumerable<Mp3Model> AddMp3ModelsFromPlaylist(string youtubePlaylistId)
        {
            return AddMp3Models(YoutubePlaylist.GetVideosFromPlaylist(youtubePlaylistId).ToArray());
        }

        private IEnumerable<Mp3Model> AddMp3Models(params string[] urls)
        {
            foreach (var url in urls)
            {
                var mp3Model = new Mp3Model
                {
                    Url = url,
                    Name = url,
                    Path = FileHelper.GetTempFileName(),
                    Quality = QualityModel.Quality,
                    State = Mp3ModelState.None
                };

                DispatchService.Invoke(() => _mp3List.Add(mp3Model));

                yield return mp3Model;
            }
        }

        private bool DownloadYoutubeVideo(Mp3Model mp3Model)
        {
            FileStream outFile = null;
            VideoDownloader videoDownloader = null;

            try
            {
                outFile = File.OpenWrite(mp3Model.Path);
                videoDownloader = new VideoDownloader(mp3Model.Url, outFile);

                mp3Model.Name = FileHelper.RemoveYoutubeSuffix(videoDownloader.CurrentVideo.Title);

                if (File.Exists(FileHelper.GetMp3FilePath(mp3Model.Name)))
                {
                    ShowInformation(string.Format(Consts.FileAlreadyExistsInfo, mp3Model.Name));
                    mp3Model.State = Mp3ModelState.Error;
                    return false;
                }

                mp3Model.State = Mp3ModelState.Downloading;

                videoDownloader.ProgressChanged += (s, a) => mp3Model.CurrentProgress = a.CurrentProgress;

                videoDownloader.Download();

                mp3Model.State = Mp3ModelState.Downloaded;

            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                ShowError(e.Message + ": " + mp3Model.Name);
                mp3Model.State = Mp3ModelState.Error;

                return false;
            }
            finally
            {
                videoDownloader?.Dispose();
                outFile?.Dispose();
            }

            return true;
        }

        private bool ConvertYoutubeVideo(Mp3Model mp3Model)
        {
            Converter converter = null;
            var srcPath = mp3Model.Path;

            try
            {
                mp3Model.State = Mp3ModelState.Converting;
                ShowInformation(string.Format(Consts.Converting, mp3Model.Name));

                converter = new Converter();

                mp3Model.CurrentProgress = 0;
                converter.ProgressChanged += (s, a) => mp3Model.CurrentProgress = a.CurrentProgress;

                mp3Model.Path = FileHelper.GetMp3FilePath(mp3Model.Name);

                FileHelper.EnsureDirectoryExist(mp3Model.Path);
                converter.ExtractAudioMp3FromVideo(srcPath, mp3Model.Path, mp3Model.Quality);

                ShowSuccess(string.Format(Consts.DoneConverting, mp3Model.Name), isShortMessage: false);

                mp3Model.State = Mp3ModelState.Done;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                ShowError(e.Message + ": " + mp3Model.Name);
                mp3Model.State = Mp3ModelState.Error;
                return false;
            }
            finally
            {
                converter?.Dispose();
                File.Delete(srcPath);
            }
           
            return true; 

        }

        #endregion

        #region Validators

        private string ValidateYoutubeUrl()
        {
            return YoutubeUrl.UrlType == YoutubeUrlType.Error ? Consts.InvalidUrlMessage : string.Empty;
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
                    ShowError(Consts.InternetConnectionError);
                }
            }
            return false;
        }

        private bool CanStartMp3Download()
        {
            return YoutubeUrl.UrlType != YoutubeUrlType.Empty && YoutubeUrl.UrlType != YoutubeUrlType.Error;
        }

        private bool CanOpenMp3Location(Mp3Model mp3Model)
        {
            return mp3Model.State == Mp3ModelState.Done;
        }

        #endregion
    }
}
