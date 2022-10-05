using System;
using System.Globalization;
using System.Windows.Data;

namespace APLan.Converters
{
    public class StringToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// convert a string value to a double value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 0;
            Double.TryParse((string)value, out result);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
