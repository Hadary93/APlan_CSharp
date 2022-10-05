using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace APLan.HelperClasses
{
    /// <summary>
    /// object to contain all information about the polylines.
    /// </summary>
    public class CustomPolyLine  : INotifyPropertyChanged
    {
        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region attributes
        //contains all information from the Eulynx Object.
        public ObservableCollection<KeyValue> Data
        {
            get;
            set;
        }
        // name as in the Eulynx Object.
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        //color is selective
        public SolidColorBrush Color
        {
            get;
            set;
        }
        //all points used to draw this polyline
        public PointCollection Points
        {
            get;
            set;
        }
        //global point represent first point found when drawing all the drawing.
        public Point GlobalPoint
        {
            get;
            set;
        }
        #endregion
        
        #region constructor
        public CustomPolyLine()
        {
            Data = new ObservableCollection<KeyValue>();
            Points = new();
        }
        #endregion

    }
}
