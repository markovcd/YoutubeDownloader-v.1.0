using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutubeDownloader
{
    class YoutubeUrlToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new YoutubeUrl(value.ToString());
        }
    }
}
