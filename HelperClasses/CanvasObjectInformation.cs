using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace APLan.HelperClasses
{
    /// <summary>
    /// Object to contain all the information needed for saving and loading processes.
    /// </summary>
    [Serializable]
    public class CanvasObjectInformation : INotifyPropertyChanged
    {

        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
        /// <summary>
        /// type of the serialized object information
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// location in the canvas before saving
        /// </summary>
        public Point LocationInCanvas { get; set; }
        /// <summary>
        /// absolute rotation in the canvas before saving
        /// </summary>
        public double Rotation { get; set; }
        /// <summary>
        /// Image source if the object is a Signal
        /// </summary>
        public string SignalImageSource { get; set; }
        /// <summary>
        /// render transofrmation for location , orientation and scaling.
        /// </summary>
        public Matrix RenderTransformMatrix { get; set; }
        /// <summary>
        /// Initial Scale before any scaling of the render transform.
        /// </summary>
        public double Scale { get; set; }
        /// <summary>
        /// included text before saving a textbox
        /// </summary>
        public string IncludedText { get; set; }
        /// <summary>
        /// included text size before saving
        /// </summary>
        public double IncludedTextSize { get; set; }
    }
}
