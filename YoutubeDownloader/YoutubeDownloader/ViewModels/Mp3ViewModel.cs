using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
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

        private YoutubeUrl _youtubeUrl;
        public YoutubeUrl YoutubeUrl
        {
            get { return _youtubeUrl; }
            set { SetProperty(ref _youtubeUrl, value); }
        }

        private bool _downloadPlaylist;
        public bool DownloadPlaylist
        {
            get { return _downloadPlaylist; }
            set { SetProperty(ref _downloadPlaylist, value); }
        }

        #endregion

        #region Commands
        public ICommand StartMp3DownloadCommand
        { 
           get { return new RelayCommand<YoutubeUrl>(StartMp3Download, CanStartMp3Download); }
        }

        public ICommand OpenMp3LocationCommand
        {
            get { return new RelayCommand<Mp3Model>(OpenMp3Location, CanOpenMp3Location); }
        }

        #endregion

        #region Constructor
        public Mp3ViewModel() : base()
        {
            Initialize();
            InitializeQualityCollection();
            InitializeValidationMappings();
        }
        #endregion

        #region Events
        private void StartMp3Download(YoutubeUrl youtubeUrl)
        {
            Task.Run(() => 
            {
                if (!CheckIfInternetConnectivityIsOn()) return;

                if (youtubeUrl.UrlType == YoutubeUrlType.Video)
                {
                    SaveVideoToDisk(youtubeUrl.Url);
                }
                else if (youtubeUrl.UrlType == YoutubeUrlType.Playlist)
                {
                    SavePlaylistToDisk(youtubeUrl.PlaylistId);
                }
                else if (DownloadPlaylist && youtubeUrl.UrlType == YoutubeUrlType.VideoAndPlaylist)
                {
                    SavePlaylistToDisk(youtubeUrl.PlaylistId);
                }
                else if (!DownloadPlaylist && youtubeUrl.UrlType == YoutubeUrlType.VideoAndPlaylist)
                {
                    SaveVideoToDisk(youtubeUrl.Url);
                }
            });
        }

        private bool CanStartMp3Download(YoutubeUrl youtubeUrl)
        {
            return youtubeUrl.UrlType != YoutubeUrlType.Empty && youtubeUrl.UrlType != YoutubeUrlType.Error;
        }

        private void OpenMp3Location(Mp3Model mp3Model)
        {
            FileHelper.OpenInExplorer(mp3Model.Path);

        }

        private bool CanOpenMp3Location(Mp3Model mp3Model)
        {
            return mp3Model.State == Mp3ModelState.Done;
        }
        #endregion

        #region Methods Private
        private void Initialize()
        {
            _connectionHelper = new ConnectionHelper();
            _cursor = new CursorControl();
            _mp3List = new ObservableCollection<Mp3Model>();
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

        private void SavePlaylistToDisk(string youtubePlaylistId)
        {
            foreach (var mp3Model in AddMp3ModelsFromPlaylist(youtubePlaylistId).ToArray())
            {
                if (DownloadYoutubeVideo(mp3Model)) ConvertYoutubeVideo(mp3Model, QualityModel.Quality);
            };
        }

        private void SaveVideoToDisk(string youtubeUrl)
        {
            var mp3Model = AddMp3Models(youtubeUrl).First();

            if (DownloadYoutubeVideo(mp3Model)) ConvertYoutubeVideo(mp3Model, QualityModel.Quality);
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
                    Path = FileHelper.GetTempFileName()
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
                    DispatchService.Invoke(() => shortToastMessage.ShowInformation(Consts.FileAlreadyExistsInfo));
                    throw new IOException(Consts.FileAlreadyExistsInfo);
                }

                mp3Model.State = Mp3ModelState.Downloading;

                videoDownloader.ProgressChanged += (s, a) => mp3Model.CurrentProgress = a.CurrentProgress;

                videoDownloader.Download();
            
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
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

        private bool ConvertYoutubeVideo(Mp3Model mp3Model, string quality)
        {
            Converter converter = null;
            var srcPath = mp3Model.Path;

            try
            {
                mp3Model.State = Mp3ModelState.Converting;
                DispatchService.Invoke(() => shortToastMessage.ShowInformation(Consts.Converting));

                converter = new Converter();

                mp3Model.CurrentProgress = 0;
                converter.ProgressChanged += (s, a) => mp3Model.CurrentProgress = a.CurrentProgress;

                mp3Model.Path = FileHelper.GetMp3FilePath(mp3Model.Name);

                FileHelper.EnsureDirectoryExist(mp3Model.Path);
                converter.ExtractAudioMp3FromVideo(srcPath, mp3Model.Path, quality);

                DispatchService.Invoke(() => longToastMessage.ShowSuccess(mp3Model.Name));

                mp3Model.State = Mp3ModelState.Done;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
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
                    shortToastMessage.ShowError(Consts.InternetConnectionError);
                }
            }
            return false;
        }

        #endregion
    }
}
