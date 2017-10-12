using System.Windows.Input;

namespace YoutubeDownloader
{
    sealed class NavigationViewModel : BaseViewModel
    {
        #region Fields and Properties
        private object _selectedViewModel;
        public object SelectedViewModel
        {
            get
            {
                return _selectedViewModel;
            }
            set
            {
                _selectedViewModel = value;
                OnPropertyChanged("SelectedViewModel");
            }
        }
        #endregion

        #region Commands
        public ICommand HomeButtonCommand
        {
            get
            {
                return new RelayCommand(HomeButtonClicked, CanExecute);
            }
        }

        public ICommand Mp3ButtonCommand
        {
            get
            {
                return new RelayCommand(Mp3ButtonClicked, CanExecute);
            }
        }
        #endregion

        #region Ctor
        public NavigationViewModel()
        {

        }
        #endregion

        #region Events
        private void HomeButtonClicked()
        {
            SelectedViewModel = new HomeViewModel();
        }

        private void Mp3ButtonClicked()
        {
            SelectedViewModel = new Mp3ViewModel();
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
