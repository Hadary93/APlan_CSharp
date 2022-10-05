using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace APLan.HelperClasses
{
    /// <summary>
    /// Node to contain all the information about the GleisKnote
    /// </summary>
    public class CustomNode : INotifyPropertyChanged
    {
        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
        #region attributes
        /// <summary>
        /// Node location after removing the global point (first point to be evaluated in the station)
        /// </summary>
        public Point NodePoint
        {
            get;
            set;
        }
        /// <summary>
        /// name of the node when hovering over.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// all the data needed to be visualized.
        /// </summary>
        public ObservableCollection<KeyValue> Data
        {
            get;
            set;
        }
        #endregion
        #region constructor
        public CustomNode()
        {
            Data = new ObservableCollection<KeyValue>();
        }
        #endregion

    }
}
