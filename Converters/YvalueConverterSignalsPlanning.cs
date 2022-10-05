using System;
using System.Globalization;
using System.Windows.Data;

namespace APLan.Converters
{
    public class YvalueConverterSignalsPlanning : IValueConverter
    {
        /// <summary>
        /// convert only a single Y coordinate component of a Signal represented in drawing.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // remove from the component the GlobalDrawingPoint y-component which represent the first point in the whole drawing to make the drawing near.
            // conver to represent cartesian coordiante.
            // shift according to the desired size of the signal.
            // then transalte it to the middle of the canvas.
            return -((double)value - ViewModels.DrawViewModel.GlobalDrawingPoint.Y) - ViewModels.DrawViewModel.signalSizeForConverter / 2 + ViewModels.DrawViewModel.sharedCanvasSize / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
