using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YoutubeDownloader
{
    sealed class NavigationViewModel : BaseViewModel
    {
        #region Fields and Properties
        private Mp3ViewModel _mp3ViewModelInstance;
        public Mp3ViewModel Mp3ViewModelInstance
        {
            get { return _mp3ViewModelInstance ?? (_mp3ViewModelInstance = new Mp3ViewModel()); }
        }

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

        private SolidColorBrush _homeBackgroundColor;
        public SolidColorBrush HomeBackgroundColor
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

        private SolidColorBrush _mp3BackgroundColor;
        public SolidColorBrush Mp3BackgroundColor
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
            SelectedViewModel = Mp3ViewModelInstance;
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
        }
        #endregion

        #region Events
        private void HomeButtonClicked()
        {
            SelectedViewModel = new HomeViewModel();
            HomeBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
        }

        private void Mp3ButtonClicked()
        {
            SelectedViewModel = Mp3ViewModelInstance;
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
            HomeBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
        }
        #endregion

        #region Methods
        #endregion
    }
}
