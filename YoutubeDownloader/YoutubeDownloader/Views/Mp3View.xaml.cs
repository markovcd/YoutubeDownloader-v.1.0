using System.Windows.Controls;

namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for Mp3View.xaml
    /// </summary>
    public partial class Mp3View : UserControl
    {
        private Mp3ViewModel _viewModel;

        public Mp3View()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            _viewModel = new Mp3ViewModel();
            this.DataContext = _viewModel;
        }
    }
}
