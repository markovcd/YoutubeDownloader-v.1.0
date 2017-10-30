using System;
using System.Windows;
using System.Windows.Input;

namespace YoutubeDownloader
{
    public sealed class CursorControl
    {
        #region Ctor
        public CursorControl()
        {
            Arrow();
        }
        #endregion
        #region Methods
        public void Wait()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });
        }

        public void Arrow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            });
        }

        public void Cross()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Cross;
            });
        }
        #endregion
    }
}
