using APLan.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace APLan.HelperClasses
{
    /// <summary>
    /// a textBox to be dragged into canvas or loaded.
    /// </summary>
    public class CustomCanvasText : TextBox
    {
        #region attributes
        public long LastTicks = 0; // for double clicking purpose.
        #endregion

        #region constructor
        public CustomCanvasText()
        {
            VerticalContentAlignment = VerticalAlignment.Center;
            HorizontalContentAlignment = HorizontalAlignment.Center;

            BorderBrush = null;
            Background = Brushes.LightGray;
            FontWeight = FontWeights.Bold;
            FontSize = DrawViewModel.signalSizeForConverter/5;
            
            //events.
            KeyDown += TxtBlock_KeyDown;
            LostFocus += TextBox_LostFocus;
            PreviewMouseLeftButtonDown += TxtBox_MouseLeftButtonDown;
            
            //attatching dependency properties to manipulate in the Canvas.
            CustomProperites.SetScale(this, 1);
            CustomProperites.SetRotation(this, 0);
        }
        #endregion

        #region logic
        /// <summary>
        /// when textbox loses focus make it read only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).IsReadOnly = true;
        }

        /// <summary>
        /// allow adjustment when double clicking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now.Ticks - LastTicks) < 3000000)
            {

                ((TextBox)sender).IsReadOnly = false;

            }
            LastTicks = DateTime.Now.Ticks;
        }

        /// <summary>
        /// read only when pressing escape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ((TextBox)sender).IsReadOnly = true;
            }
        }
        #endregion
    }
}
