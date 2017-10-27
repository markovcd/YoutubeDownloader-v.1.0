using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeDownloader
{
    public class Converter
    {
        #region Fields & Properties
        private readonly string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
        private Process ffmpegProcess;
        private const string _temporaryFolderName = "YouTubeDownloaderTEMP";
        private const string _defaultFolderName = "YouTubeDownloader";
        #endregion

        #region Ctor
        public Converter()
        {
            ffmpegProcess = new Process();
        }
        #endregion

        #region Methods
        public void ExtractAudioMp3FromVideo(string videoToWorkWith)
        {
            try
            {
                var inputFile = videoToWorkWith;
                var tmp = videoToWorkWith.Replace(".mp4", ".mp3");
                var outputFile = tmp.Replace(_temporaryFolderName, _defaultFolderName);
                var mp3output = string.Empty;
                
                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.RedirectStandardInput = true;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.CreateNoWindow = true;
                ffmpegProcess.StartInfo.FileName = ffmpegExePath;

                // attachted events
                ffmpegProcess.EnableRaisingEvents = true;
                ffmpegProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceivedOccured);
                ffmpegProcess.Exited += new EventHandler(ExitedOccured);
                ffmpegProcess.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceivedOccured);

                // TIP! Refer to https://trac.ffmpeg.org/wiki/Encode/MP3 for more infor about arguments you get use
                // or https://gist.github.com/protrolium/e0dbd4bb0f1a396fcb55 
                ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -codec:a libmp3lame -qscale:a 2 " + outputFile;

                ffmpegProcess.Start();
                //ffmpegProcess.BeginErrorReadLine();
                //ffmpegProcess.BeginOutputReadLine();
                ffmpegProcess.StandardOutput.ReadToEnd();
                mp3output = ffmpegProcess.StandardError.ReadToEnd();
                ffmpegProcess.WaitForExit();

                Debug.WriteLine(mp3output);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Occured: {0}", e);
            }
        }
        #endregion

        #region Events
        private void ExitedOccured(object sender, EventArgs e)
        {
            // TODO: 
        }

        private void ErrorDataReceivedOccured(object sender, DataReceivedEventArgs e)
        {
            // TODO: 
        }

        private void OutputDataReceivedOccured(object sender, DataReceivedEventArgs e)
        {
            // TODO: 
        }
        #endregion
    }
}
