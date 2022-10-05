using System;
using System.Globalization;
using System.Windows.Data;

namespace APLan.Converters
{
    internal class MultiBindingConverter : IMultiValueConverter
    {
        /// <summary>
        /// clone the objects needed for binding to handle them later.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
