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
            
        }

        public SettingsModel Model { get { return SettingsSingleton.Instance.Model; } }
    }
}
