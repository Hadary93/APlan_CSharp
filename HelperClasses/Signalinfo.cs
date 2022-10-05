using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace APLan.HelperClasses
{
    /// <summary>
    /// object to contain all information about the Signal to be represented whenever needed.
    /// this information is related to the Eulynx Object.
    /// </summary>
    public class Signalinfo
    {
        public double? LateralDistance
        {
            get;
            set;
        }
        public string Function
        {
            get;
            set;
        }
        public string Type
        {
            get;
            set;
        }
        public BitmapImage SignalImageSource
        {
            get;
            set;
        }
        public double? AttachedToElementLength
        {
            get;
            set;
        }
        public string Side
        {
            get;
            set;
        }
        public double IntrinsicValue
        {
            get;
            set;
        }
        public string AttachedToElementname
        {
            get;
            set;
        }
        public string LongName
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public Point LocationCoordinate
        {
            get;
            set;
        }
        public double Orientation
        {
            get;
            set;
        }
        public List<Point> Coordinates
        {
            get;
            set;
        }
        public string Direction
        {
            get;
            set;
        }
    }
}
