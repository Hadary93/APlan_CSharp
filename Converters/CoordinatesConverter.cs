using APLan.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace APLan.Converters
{
    public class CoordinatesConverter : IValueConverter
    {
        /// <summary>
        /// convert all points of a polyline due to difference in Canvas coordinates system, and translate them to the middle of the Canvas.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
      
            PointCollection points = new PointCollection();

            for (int i = 0; i < ((PointCollection)value).Count; i++)
            {
                // remove from the point the GlobalDrawingPoint which represent the first point in the whole drawing to make the drawing near.
                // then convert it to be represented as cartesian coodrinate
                // then transalte it to the middle of the canvas
                points.Add( new Point((((PointCollection)value)[i].X  - DrawViewModel.GlobalDrawingPoint.X) + DrawViewModel.sharedCanvasSize / 2,
                                      -(((PointCollection)value)[i].Y - DrawViewModel.GlobalDrawingPoint.Y) + DrawViewModel.sharedCanvasSize / 2));
            }
            return points;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
