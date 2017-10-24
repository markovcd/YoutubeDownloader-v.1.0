using System;
using System.Net;
using System.Net.NetworkInformation;

namespace YoutubeDownloader
{
    public sealed class ConnectionHelper
    {
        public ConnectionHelper()
        {

        }

        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
