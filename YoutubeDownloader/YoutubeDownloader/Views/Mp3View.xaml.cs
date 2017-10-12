using System.Windows.Controls;

namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for Mp3View.xaml
    /// </summary>
    public partial class Mp3View : UserControl
    {
        public Mp3View()
        {
            InitializeComponent();
            this.DataContext = new Mp3ViewModel();
        }
    }
}
