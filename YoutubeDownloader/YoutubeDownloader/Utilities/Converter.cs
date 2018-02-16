using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace YoutubeDownloader
{
    enum ConversionSection { Input, Output }

    public class Converter
    {
        #region Fields & Properties
        private readonly string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
        private TimeSpan _totalDuration;
        private TimeSpan _currentDuration;
        private ConversionSection _conversionSection;
        #endregion

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public double CurrentProgress { get; private set; }

        #region Methods
        public void ExtractAudioMp3FromVideo(string inputFile, string outputFile, string quality)
        {
            try
            {
                using (var process = new Process())

                {

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = ffmpegExePath;


                    process.EnableRaisingEvents = true;
                    process.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);
                    process.Exited += new EventHandler(OnConversionExited);

                    process.StartInfo.Arguments = " -i \"" + inputFile + "\" -codec:a libmp3lame -b:a " + quality + " \"" + outputFile + "\"";

                    process.Start();
                    process.BeginErrorReadLine();


                    process.WaitForExit();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Occured: {0}", e);
            }
        }

        private Match ParseTotalDurationLine(string data)
        {
            return Regex.Match(data, @"(  Duration: )(\d+:\d+:\d+.\d+)(, start: )(\d.\d+)(, bitrate: )(\d+)( kb/s)");
        }

        private Match ParseConversionProgressLine(string data)
        {
            return Regex.Match(data, @"(size=\s+)(\d+kB)( time=)(\d+:\d+:\d+.\d+)( bitrate=\s+)(\d+.\d+kbits/s)( speed=)(\d+.\d+x)(\s+)");
        }

        #endregion

        #region Events
        private void OnConversionExited(object sender, EventArgs e)
        {
            Debug.WriteLine("OnConversionExited");
        }

        protected virtual void OnProgressChanged(ProgressEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }


        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            const string inputIndicator = "Input #0, ";
            const string outputIndicator = "Output #0, ";

            if (e.Data.StartsWith(inputIndicator)) _conversionSection = ConversionSection.Input;
            if (e.Data.StartsWith(outputIndicator)) _conversionSection = ConversionSection.Output;

            if (_conversionSection == ConversionSection.Input)
            {
                var match = ParseTotalDurationLine(e.Data);
                if (match.Value != String.Empty) _totalDuration = TimeSpan.Parse(match.Groups[2].Value);
            }
            else if (_conversionSection == ConversionSection.Output)
            {
                var match = ParseConversionProgressLine(e.Data);
                if (match.Value != String.Empty)
                {
                    _currentDuration = TimeSpan.Parse(match.Groups[4].Value);
                    OnProgressChanged(new ProgressEventArgs(_currentDuration.TotalMilliseconds * 100 / _totalDuration.TotalMilliseconds));
                }
            }

            // TODO: implement logger
            //Debug.WriteLine("Input line: {0} ({1:m:s:fff})", _currentLine++, DateTime.Now);
            Debug.WriteLine(e.Data);
        }

        #endregion
    }

}
