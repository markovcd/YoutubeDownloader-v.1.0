using System.Collections.Generic;
using System.Windows.Input;
using ToastNotifications.Messages;

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
            this.YoutubeLinkUrl = Consts.DefaultTextBoxEntry;
            this._connectionHelper = new ConnectionHelper();
        }
        #endregion

        #region Events
        private void GoButtonClicked()
        {
            if (ValidateEditFieldString())
            {
                // TODO: Implement whole downloading logic
            }
        }
        #endregion

        #region Methods
        private void SaveVideoToDisk(string link)
        {
            // TODO: Check if there's an internet connection
            // if there is.. than start downloading the file

            //if (_connectionHelper != null)
            //{
            //    if (_connectionHelper.CheckForInternetConnection())
            //    {

            //        //string tmp = "http://www.youtube.com/watch?v=6bITHU0srt4";
            //        //var youTube = YouTube.Default; // starting point for YouTube actions
            //        //var video = youTube.GetVideo(tmp); // gets a Video object with info about the video

            //        //if (fileHelper != null)
            //        //{
            //        //    fileHelper.WriteToFile(video.FullName, video.GetBytes());
            //        //}
            //        //notifier.ShowSuccess("Zajebiscie wyszlo");
            //    }
            //    else
            //    {
            //        notifier.ShowError(Consts.InternetConnectionError);
            //    }
            //}
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

        private bool CanExecute()
        {
            // TODO: some logic
            return true;
        }
        #endregion
    }
}
