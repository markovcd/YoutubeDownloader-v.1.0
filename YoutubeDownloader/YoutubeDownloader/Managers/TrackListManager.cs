using System;
using System.Collections.Generic;

namespace YoutubeDownloader
{
    public sealed class TrackListManager
    {
        private static volatile TrackListManager _instance;
        private static List<VideoModel> _trackList;
        private static object syncRoot = new Object();

        private TrackListManager() { }

        public static TrackListManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new TrackListManager();
                            _trackList = new List<VideoModel>();
                        }
                    }
                }

                return _instance;
            }
        }

        public void UpdateList(VideoModel videoModel)
        {
            if (_trackList != null)
            {
                _trackList.Add(videoModel);
            }
        }

        public List<VideoModel> GetItems()
        {
            if (_trackList != null && _trackList.Count > 0)
            {
                return _trackList;
            }
            else
            {
                return null;
            }
        }
    }
}
