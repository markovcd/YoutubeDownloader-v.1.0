using System;
using VideoLibrary;
using System.IO;

namespace YoutubeDownloader
{
    public class ProgressEventArgs : EventArgs
    {
        public double CurrentProgress { get; }

        public ProgressEventArgs(double currentProgress)
        {
            CurrentProgress = currentProgress;
        }
    }

    public struct VideoInfo
    {
        public string FileExtension { get; }
        public VideoFormat Format { get; }
        public string FullName { get; }
        public string Title { get; }
        public WebSites WebSite { get; }

        public VideoInfo(Video video)
        {
            FileExtension = video.FileExtension;
            Format = video.Format;
            FullName = video.FullName;
            Title = video.Title;
            WebSite = video.WebSite;
        }
    }

    public class VideoDownloader : IDisposable
    {
        public string YoutubeLinkUrl { get; }

        public Stream TargetStream { get; }

        public double CurrentProgress { get; private set; }

        public VideoInfo CurrentVideo { get; }

        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler DownloadStarted;
        public event EventHandler DownloadFinished;
        public event ErrorEventHandler DownloadError;

        private Video _video;
        private Client<YouTubeVideo> _service;

        public VideoDownloader(string youtubeLinkUrl, Stream targetStream)
        {
            YoutubeLinkUrl = youtubeLinkUrl;
            TargetStream = targetStream;
            CurrentProgress = 0;

            _service = Client.For(YouTube.Default);
            _video = _service.GetVideo(youtubeLinkUrl);

            CurrentVideo = new VideoInfo(_video);      
        }

        public void Download()
        {
            try
            {
                OnDownloadStarted(new EventArgs());
                
                using (var progressStream = new ProgressStream(TargetStream))
                {
                    var streamLength = (long)_video.StreamLength();

                    progressStream.BytesMoved += (s, a) =>
                    {
                        CurrentProgress = a.StreamLength * 100 / streamLength;
                        OnProgressChanged(new ProgressEventArgs(CurrentProgress));
                    };

                    _video.Stream().CopyTo(progressStream);
                }
               
            }
            catch (Exception e)
            {

                OnDownloadError(new ErrorEventArgs(e));
                throw e;
            }


            OnDownloadFinished(new EventArgs());
        }

        protected virtual void OnProgressChanged(ProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnDownloadStarted(EventArgs e)
        {
            DownloadStarted?.Invoke(this, e);
        }

        protected virtual void OnDownloadFinished(EventArgs e)
        {
            DownloadFinished?.Invoke(this, e);
        }

        protected virtual void OnDownloadError(ErrorEventArgs e)
        {
            DownloadError?.Invoke(this, e);
        }

        public void Dispose()
        {
            _video.Dispose();
            _service.Dispose();
        }
    }
}
