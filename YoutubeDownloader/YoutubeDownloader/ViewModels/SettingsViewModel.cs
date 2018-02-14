using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeDownloader
{
    sealed class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            System.Diagnostics.Debug.Print(SettingsSingleton.Instance.Model.Mp3DestinationPath);
        }
    }
}
