using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using APLan.ViewModels;
using System.Collections;

namespace APLan.Views
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : Window
    {
        public NewProjectWindow()
        {
            InitializeComponent();
        }
        private void fileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(createProject!=null)
            createProject.IsEnabled = false;
            if ( ((System.Windows.Controls.ComboBox)sender).SelectedItem.ToString().Equals(".json"))
            {
                container.RowDefinitions[5].Height = new GridLength(0);
                container.RowDefinitions[6].Height = new GridLength(0);
                container.RowDefinitions[7].Height = new GridLength(0);
                for (int i = 4; i < container.RowDefinitions.Count - 4; i++)
                {
                    container.RowDefinitions[i].Height = new GridLength(1, GridUnitType.Star);
                }
            }
            else if (((System.Windows.Controls.ComboBox)sender).SelectedItem.ToString().Equals(".mdb"))
            {
                for (int i=4; i< container.RowDefinitions.Count-1; i++)
                {
                    container.RowDefinitions[i].Height = new GridLength(0);
                }
                container.RowDefinitions[5].Height = new GridLength(1, GridUnitType.Star);
            }
            else if (((System.Windows.Controls.ComboBox)sender).SelectedItem.ToString().Equals(".euxml"))
            {
                for (int i = 4; i < container.RowDefinitions.Count-1; i++)
                {
                    container.RowDefinitions[i].Height = new GridLength(0);
                }
                container.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);
            }
            else if (((System.Windows.Controls.ComboBox)sender).SelectedItem.ToString().Equals(".ppxml"))
            {
                for (int i = 4; i < container.RowDefinitions.Count - 1; i++)
                {
                    container.RowDefinitions[i].Height = new GridLength(0);
                }
                container.RowDefinitions[7].Height = new GridLength(1, GridUnitType.Star);
            }
        }
    }
}
