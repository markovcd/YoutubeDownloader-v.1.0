using System;
using System.Globalization;
using System.Windows;

namespace YoutubeDownloader
{
    class Mp3ModelStateToProgressVisibilityConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (Mp3ModelState)value;

            switch (state)
            {
                case Mp3ModelState.None:
                    break;
                case Mp3ModelState.Downloading:
                    return Visibility.Visible;
                case Mp3ModelState.Converting:
                    return Visibility.Visible;
                case Mp3ModelState.Done:
                    return Visibility.Hidden;
                case Mp3ModelState.Error:
                    return Visibility.Hidden;
            }

            throw new InvalidOperationException();
        }
    }

    class Mp3ModelStateToOperationDoneVisibilityConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (Mp3ModelState)value;

            switch (state)
            {
                case Mp3ModelState.None:
                    break;
                case Mp3ModelState.Downloading:
                    return Visibility.Hidden;
                case Mp3ModelState.Converting:
                    return Visibility.Hidden;
                case Mp3ModelState.Done:
                    return Visibility.Visible;
                case Mp3ModelState.Error:
                    return Visibility.Visible;
            }

            throw new InvalidOperationException();
        }
    }


}
