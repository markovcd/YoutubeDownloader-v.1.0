using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutubeDownloader
{
    [ValueConversion(typeof(String), typeof(String))]

    public class ProgressBarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                String str = value as String;
                String strValue = str;
                strValue += "%";
                return strValue;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
