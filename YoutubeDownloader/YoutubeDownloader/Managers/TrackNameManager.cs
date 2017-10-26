﻿using System;

namespace YoutubeDownloader
{
    public sealed class TrackNameManager
    {
        private static volatile TrackNameManager instance;
        private static object syncRoot = new Object();
        private string _defaultTrackName = string.Empty;

        private TrackNameManager() { }

        public static TrackNameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TrackNameManager();
                    }
                }

                return instance;
            }
        }

        public string DefaultTrackName
        {
            get
            {
                return _defaultTrackName;
            }

            set
            {
                _defaultTrackName = value;
            }
        }
    }
}
