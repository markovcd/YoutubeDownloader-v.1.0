using System.Diagnostics;
using System.Windows.Input;

namespace YoutubeDownloader
{
    class Mp3ViewModel : BaseViewModel
    {
        #region Fields and Properties
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
            this.YoutubeLinkUrl = "Enter your link here!";
        }
        #endregion

        #region Events
        private void GoButtonClicked()
        {
            // TODO: write an validator for the string
            if (YoutubeLinkUrl != string.Empty)
            {
                // TODO: perform action
            }
        }
        #endregion

        #region Methods
        private bool CanExecute()
        {
            // TODO: some logic
            return true;
        }
        #endregion
    }
}
