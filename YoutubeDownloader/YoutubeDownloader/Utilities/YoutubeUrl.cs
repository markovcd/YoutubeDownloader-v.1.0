using System.Text.RegularExpressions;

namespace YoutubeDownloader
{
    enum YoutubeUrlType { Empty, Error, Video, Playlist, VideoAndPlaylist }

    struct YoutubeUrl
    {
        private const string _regexVideoAndPlaylist = @"https:\/\/www\.youtube\.com\/watch\?v=([a-zA-Z0-9_-]+)&list=([a-zA-Z0-9_-]+)($|(&.+))";
        private const string _regexPlaylist = @"https:\/\/www\.youtube\.com\/playlist\?list=([a-zA-Z0-9_-]+)($|(&.+))";
        private const string _regexVideo = @"https:\/\/www\.youtube\.com\/watch\?v=([a-zA-Z0-9_-]+)($|(&.+))";

        public string VideoId { get; }
        public string PlaylistId { get; }
        public string Url { get; }
        public YoutubeUrlType UrlType { get; }

        public YoutubeUrl(string url)
        {
            Url = url;

            var matchVideoAndPlaylist = Regex.Match(url, _regexVideoAndPlaylist);
            var matchVideo = Regex.Match(url, _regexVideo);
            var matchPlaylist = Regex.Match(url, _regexPlaylist);

            if (matchVideoAndPlaylist.Value != string.Empty)
            {
                UrlType = YoutubeUrlType.VideoAndPlaylist;
                VideoId = matchVideoAndPlaylist.Groups[1].Value;
                PlaylistId = matchVideoAndPlaylist.Groups[2].Value;
            }
            else if (matchVideo.Value != string.Empty)
            {
                UrlType = YoutubeUrlType.Video;
                VideoId = matchVideo.Groups[1].Value;
                PlaylistId = string.Empty;
            }
            else if (matchPlaylist.Value != string.Empty)
            {
                UrlType = YoutubeUrlType.Playlist;
                VideoId = string.Empty;
                PlaylistId = matchPlaylist.Groups[1].Value;
            }
            else
            {
                UrlType = url.Trim() == string.Empty ? YoutubeUrlType.Empty : YoutubeUrlType.Error;
                VideoId = string.Empty;
                PlaylistId = string.Empty;
            }
        }

        public override string ToString()
        {
            return Url?.ToString();
        }

    }
}
