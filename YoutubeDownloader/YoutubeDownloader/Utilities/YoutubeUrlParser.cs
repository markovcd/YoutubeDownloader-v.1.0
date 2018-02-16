using System.Text.RegularExpressions;

namespace YoutubeDownloader
{
    enum YoutubeUrlType { Empty, Error, Video, Playlist, VideoAndPlaylist }

    class YoutubeUrlParser
    {
        private const string _regexVideoAndPlaylist = @"https:\/\/www\.youtube\.com\/watch\?v=([a-zA-Z0-9_-]+)&list=([a-zA-Z0-9_-]+)($|(&.+))";
        private const string _regexPlaylist = @"https:\/\/www\.youtube\.com\/playlist\?list=([a-zA-Z0-9_-]+)($|(&.+))";
        private const string _regexVideo = @"https:\/\/www\.youtube\.com\/watch\?v=([a-zA-Z0-9_-]+)$";

        public string VideoId { get; private set; }
        public string PlaylistId { get; private set; }

        public YoutubeUrlType UrlType { get; private set; }

        private string _url;
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value.Trim();

                var matchVideoAndPlaylist = Regex.Match(_url, _regexVideoAndPlaylist);
                var matchVideo = Regex.Match(_url, _regexVideo);
                var matchPlaylist = Regex.Match(_url, _regexPlaylist);

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
                    UrlType = _url == string.Empty ? YoutubeUrlType.Empty : YoutubeUrlType.Error;
                    VideoId = string.Empty;
                    PlaylistId = string.Empty;
                }
            }
        }
    }
}
