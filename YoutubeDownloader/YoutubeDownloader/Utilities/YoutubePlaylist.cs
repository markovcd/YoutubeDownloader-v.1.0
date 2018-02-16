using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace YoutubeDownloader.Utilities
{
    class YoutubePlaylist
    {

        private const string _youtubeUrl = "https://www.youtube.com/watch?v={0}";
        private const string _apiKey = "AIzaSyBbJUQUHzgTfj6oeCPEUUXagoFUXdqN8Ig"; // project Api key, do not touch!
        private const string _apiUrl = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&playlistId={0}&key={1}&pageToken={2}&maxResults={3}";

        private const string _playlistRegex1 = @"https:\/\/www\.youtube\.com\/watch\?v=[a-zA-Z0-9_-]+&list=([a-zA-Z0-9_-]+)($|(&.+))";
        private const string _playlistRegex2 = @"https:\/\/www\.youtube\.com\/playlist\?list=([a-zA-Z0-9_-]+)($|(&.+))";

        private string FormatApiUrl(string playlistId, string pageToken, int maxResults = 20)
        {
            return string.Format(_apiUrl, playlistId, _apiKey, pageToken, maxResults);
        }

        private string FormatYoutubeUrl(string videoId)
        {
            return string.Format(_youtubeUrl, videoId);
        }

        private string ParsePlaylistUrl(string url, string pattern)
        {
            var match = Regex.Match(url, pattern);
            if (match.Value == string.Empty) return string.Empty;

            return match.Groups[1].Value;
        }

        private string ParsePlaylistUrl(string playlistUrl)
        {
            var playlistId1 = ParsePlaylistUrl(playlistUrl, _playlistRegex1);
            if (playlistId1 != string.Empty) return playlistId1;

            var playlistId2 = ParsePlaylistUrl(playlistUrl, _playlistRegex2);
            if (playlistId2 != string.Empty) return playlistId2;

            throw new ArgumentException(nameof(playlistUrl));
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

        public IEnumerable<string> GetVideosFromPlaylist(string playlistUrl)
        {
            var playlistId = ParsePlaylistUrl(playlistUrl);

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
