using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net;


namespace YoutubeDownloader.Utilities
{

    class YoutubePlaylist
    {

        private const string _youtubeUrl = "https://www.youtube.com/watch?v={0}";
        private const string _apiKey = "AIzaSyBbJUQUHzgTfj6oeCPEUUXagoFUXdqN8Ig"; // project Api key, do not touch!
        private const string _apiUrl = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&playlistId={0}&key={1}&pageToken={2}&maxResults={3}";

       
        private string FormatApiUrl(string playlistId, string pageToken, int maxResults = 20)
        {
            return string.Format(_apiUrl, playlistId, _apiKey, pageToken, maxResults);
        }

        private string FormatYoutubeUrl(string videoId)
        {
            return string.Format(_youtubeUrl, videoId);
        }

        public JObject GetPlaylistData(string playlistId, string pageToken, int maxResults = 20)
        {
            var http = new WebClient();
            var data = http.DownloadString(FormatApiUrl(playlistId, pageToken, maxResults));
            return JObject.Parse(data);
        }

        private IEnumerable<JObject> GetPlaylistDataRecursive(string playlistId, string pageToken, int maxResults = 20)
        {
            var data = GetPlaylistData(playlistId, pageToken, maxResults);

            yield return data;

            var nextPageToken = data["nextPageToken"];

            if (nextPageToken != null)
            {
                foreach (var data2 in GetPlaylistDataRecursive(playlistId, nextPageToken.Value<string>(), maxResults))
                {
                    yield return data2;
                }
            }
        }

        public IEnumerable<JObject> GetAllPlaylistData(string playlistId, int maxResults = 20) => GetPlaylistDataRecursive(playlistId, "", maxResults);

        public IEnumerable<string> GetVideosFromPlaylist(string playlistId)
        {
            var data = GetAllPlaylistData(playlistId, 50);

            foreach (var part in data)
            {
                foreach (var item in part["items"])
                {
                    var videoId = item["contentDetails"]["videoId"].Value<string>();
                    yield return FormatYoutubeUrl(videoId);
                }
            }

        }
    }

}
