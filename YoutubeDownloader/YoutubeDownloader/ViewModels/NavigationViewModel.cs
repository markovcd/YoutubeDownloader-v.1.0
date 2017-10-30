using System.Windows.Input;
using System.Windows.Media;

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

        private Brush _homeBackgroundColor;
        public Brush HomeBackgroundColor
        {
            get
            {
                return _homeBackgroundColor;
            }
            set
            {
                _homeBackgroundColor = value;
                OnPropertyChanged("HomeBackgroundColor");
            }
        }

        private Brush _mp3BackgroundColor;
        public Brush Mp3BackgroundColor
        {
            get
            {
                return _mp3BackgroundColor;
            }
            set
            {
                _mp3BackgroundColor = value;
                OnPropertyChanged("Mp3BackgroundColor");
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
            SelectedViewModel = new HomeViewModel();
            HomeBackgroundColor = Brushes.DeepSkyBlue;
        }
        #endregion

        #region Events
        private void HomeButtonClicked()
        {
            SelectedViewModel = new HomeViewModel();
            HomeBackgroundColor = Brushes.DeepSkyBlue;
            Mp3BackgroundColor = Brushes.White;
        }

        private void Mp3ButtonClicked()
        {
            SelectedViewModel = new Mp3ViewModel();
            Mp3BackgroundColor = Brushes.DeepSkyBlue;
            HomeBackgroundColor = Brushes.White;
        }
        #endregion

        #region Methods
        #endregion
    }
}
