using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeDownloader
{
    public class Converter
    {
        #region Fields & Properties
        private readonly string ffmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg\\ffmpeg.exe");

        
        #endregion

 

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
                    process.OutputDataReceived += new DataReceivedEventHandler(OnOutputDataReceived);


                    process.StartInfo.Arguments = " -i \"" + inputFile + "\" -codec:a libmp3lame -b:a " + quality + " \"" + outputFile + "\"";

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();


                    process.WaitForExit();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Occured: {0}", e);
            }
        }


        #endregion

        #region Events
        private void OnConversionExited(object sender, EventArgs e)
        {
            Debug.WriteLine("OnConversionExited");
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: implement logger
            //Debug.WriteLine("Input line: {0} ({1:m:s:fff})", _currentLine++, DateTime.Now);
            Debug.WriteLine(e.Data);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // TODO: implement logger
            //Debug.WriteLine("Input line: {0} ({1:m:s:fff})", _currentLine++, DateTime.Now);
            Debug.WriteLine(e.Data);
        }

        private void OutputDataReceivedOccured(object sender, DataReceivedEventArgs e)
        {
            // TODO: 
        }
        #endregion
    }

}
