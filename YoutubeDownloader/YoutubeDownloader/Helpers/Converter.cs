using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeDownloader
{
    public class Converter
    {
        private readonly string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");

        public Converter()
        {

        }

        public void ExtractAudioMp3FromVideo(string videoToWorkWith)
        {
            var inputFile = videoToWorkWith;
            var outputFile = videoToWorkWith.Replace(".mp4", ".mp3");
            var mp3output = string.Empty;

            var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.FileName = ffmpegExePath;

            // TIP! Refer to https://trac.ffmpeg.org/wiki/Encode/MP3 for more infor about arguments you get use
            // or https://gist.github.com/protrolium/e0dbd4bb0f1a396fcb55 
            ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -codec:a libmp3lame -qscale:a 2 " + outputFile;

            ffmpegProcess.Start();
            ffmpegProcess.StandardOutput.ReadToEnd();
            mp3output = ffmpegProcess.StandardError.ReadToEnd();
            ffmpegProcess.WaitForExit();

            if (!ffmpegProcess.HasExited)
            {
                ffmpegProcess.Kill();
            }
            Debug.WriteLine(mp3output);
            ffmpegProcess.Dispose();
        }
    }
}
