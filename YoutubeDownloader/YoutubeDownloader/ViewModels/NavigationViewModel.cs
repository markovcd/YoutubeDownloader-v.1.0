using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YoutubeDownloader
{
    sealed class NavigationViewModel : BaseViewModel
    {
        #region Fields and Properties
        private HomeViewModel _homeViewModelInstance;
        public HomeViewModel HomeViewModelInstance => _homeViewModelInstance ?? (_homeViewModelInstance = new HomeViewModel());

        private Mp3ViewModel _mp3ViewModelInstance;
        public Mp3ViewModel Mp3ViewModelInstance  => _mp3ViewModelInstance ?? (_mp3ViewModelInstance = new Mp3ViewModel());

        private Mp4ViewModel _mp4ViewModelInstance;
        public Mp4ViewModel Mp4ViewModelInstance => _mp4ViewModelInstance ?? (_mp4ViewModelInstance = new Mp4ViewModel());

        private SettingsViewModel _settingsViewModelInstance;
        public SettingsViewModel SettingsViewModelInstance => _settingsViewModelInstance ?? (_settingsViewModelInstance = new SettingsViewModel());

        private object _selectedViewModel;
        public object SelectedViewModel
        {
            get => _selectedViewModel;
            set => SetProperty(ref _selectedViewModel, value);
        }

        private SolidColorBrush _homeBackgroundColor;
        public SolidColorBrush HomeBackgroundColor
        {
            get => _homeBackgroundColor;
            set => SetProperty(ref _homeBackgroundColor, value);
        }

        private SolidColorBrush _mp3BackgroundColor;
        public SolidColorBrush Mp3BackgroundColor
        {
            get => _mp3BackgroundColor;
            set => SetProperty(ref _mp3BackgroundColor, value);
        }

        private SolidColorBrush _mp4BackgroundColor;
        public SolidColorBrush Mp4BackgroundColor
        {
            get => _mp4BackgroundColor;
            set => SetProperty(ref _mp4BackgroundColor, value);
        }

        private SolidColorBrush _settingsBackgroundColor;
        public SolidColorBrush SettingsBackgroundColor
        {
            get => _settingsBackgroundColor;
            set => SetProperty(ref _settingsBackgroundColor, value);
        }
        #endregion

        #region Commands
        public ICommand HomeButtonCommand => new RelayCommand(HomeButtonClicked);        
        public ICommand Mp3ButtonCommand => new RelayCommand(Mp3ButtonClicked);
        public ICommand Mp4ButtonCommand => new RelayCommand(Mp4ButtonClicked);
        public ICommand SettingsButtonCommand => new RelayCommand(SettingsButtonClicked);
        #endregion

        #region Ctor
        public NavigationViewModel() : base()
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
