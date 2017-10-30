using System.Windows.Controls;

namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private HomeViewModel _viewModel;

        public HomeView()
        {
            InitializeComponent();
            Setup();
        }

        public void Setup()
        {
            _viewModel = new HomeViewModel();
            this.DataContext = _viewModel;
        }
    }
}
