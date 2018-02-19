using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace YoutubeDownloader
{
    enum ConversionSection { Input, Output }

    public class Converter : IDisposable
    {
        #region Fields & Properties
        private readonly string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");
        private TimeSpan _totalDuration;
        private TimeSpan _currentDuration;
        private ConversionSection _conversionSection;
        private Process _process;
        #endregion

        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public double CurrentProgress { get; private set; }

        public Converter()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = ffmpegExePath
            };

            _process = new Process() {
                StartInfo = startInfo,
                EnableRaisingEvents = true,

            };

            _process.EnableRaisingEvents = true;
            _process.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);
            _process.Exited += new EventHandler(OnConversionExited);

        }

        #region Methods
        public void ExtractAudioMp3FromVideo(string inputFile, string outputFile, string quality)
        {
            try
            {
                _process.StartInfo.Arguments = $" -i \"{inputFile}\" -codec:a libmp3lame -b:a {quality} \"{outputFile}\"";

                _process.Start();
                _process.BeginErrorReadLine();

                _process.WaitForExit();

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

        public void Dispose()
        {
            if (_process != null && !_process.HasExited) _process.Kill();
            _process.Dispose();
        }

        #endregion
    }

}
