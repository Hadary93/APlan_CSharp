using System;
using System.Globalization;
using System.Windows.Data;

namespace APLan.Converters
{
    public class YvalueConverter : IValueConverter
    {
        /// <summary>
        /// convert only a single Y coordinate component 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // remove from the component the GlobalDrawingPoint x-component which represent the first point in the whole drawing to make the drawing near.
            // convert to represent a cartesian coordinate.
            // then transalte it to the middle of the canvas.
            return -((double)value - ViewModels.DrawViewModel.GlobalDrawingPoint.Y) + ViewModels.DrawViewModel.sharedCanvasSize / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
