using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YoutubeDownloader
{
    sealed class NavigationViewModel : BaseViewModel
    {
        #region Fields and Properties
        private HomeViewModel _homeViewModelInstance;
        public HomeViewModel HomeViewModelInstance
        {
            get { return _homeViewModelInstance ?? (_homeViewModelInstance = new HomeViewModel()); }
        }

        private Mp3ViewModel _mp3ViewModelInstance;
        public Mp3ViewModel Mp3ViewModelInstance
        {
            get { return _mp3ViewModelInstance ?? (_mp3ViewModelInstance = new Mp3ViewModel()); }
        }

        private Mp4ViewModel _mp4ViewModelInstance;
        public Mp4ViewModel Mp4ViewModelInstance
        {
            get { return _mp4ViewModelInstance ?? (_mp4ViewModelInstance = new Mp4ViewModel()); }
        }

        private SettingsViewModel _settingsViewModelInstance;
        public SettingsViewModel SettingsViewModelInstance
        {
            get { return _settingsViewModelInstance ?? (_settingsViewModelInstance = new SettingsViewModel()); }
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

        private SolidColorBrush _mp4BackgroundColor;
        public SolidColorBrush Mp4BackgroundColor
        {
            get
            {
                return _mp4BackgroundColor;
            }
            set
            {
                _mp4BackgroundColor = value;
                OnPropertyChanged("Mp4BackgroundColor");
            }
        }

        private SolidColorBrush _settingsBackgroundColor;
        public SolidColorBrush SettingsBackgroundColor
        {
            get
            {
                return _settingsBackgroundColor;
            }
            set
            {
                _settingsBackgroundColor = value;
                OnPropertyChanged("SettingsBackgroundColor");
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

        public ICommand Mp4ButtonCommand
        {
            get
            {
                return new RelayCommand(Mp4ButtonClicked, CanExecute);
            }
        }

        public ICommand SettingsButtonCommand
        {
            get
            {
                return new RelayCommand(SettingsButtonClicked, CanExecute);
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
            SelectedViewModel = HomeViewModelInstance;
            HomeBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
            Mp4BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            SettingsBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
        }

        private void Mp3ButtonClicked()
        {
            SelectedViewModel = Mp3ViewModelInstance;
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
            Mp4BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            HomeBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            SettingsBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
        }

        private void Mp4ButtonClicked()
        {
            SelectedViewModel = Mp4ViewModelInstance;
            Mp4BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            HomeBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            SettingsBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
        }

        private void SettingsButtonClicked()
        {
            SelectedViewModel = SettingsViewModelInstance;
            Mp4BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            Mp3BackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            HomeBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitSoftColor"];
            SettingsBackgroundColor = (SolidColorBrush)Application.Current.Resources["GrafitColor"];
        }
        #endregion

        #region Methods
        #endregion
    }
}
