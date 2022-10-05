using APLan.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APLan.Views
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = ((MenuItem)sender);
            if (item.IsChecked)
            {
                ((MenuItem)sender).IsChecked = false;
                APLan.Views.Draw.visualizedDataColumn.Width = new GridLength(0,GridUnitType.Star);
            }
            else
            {
                ((MenuItem)sender).IsChecked = true;
                APLan.Views.Draw.visualizedDataColumn.Width = new GridLength(1, GridUnitType.Star);
            }
            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuItem item = ((MenuItem)sender);
            if (item.IsChecked)
            {
                ((MenuItem)sender).IsChecked = false;
                MainWindow.rightTabTargetColumn.Width = new GridLength(0, GridUnitType.Star);
            }
            else
            {
                ((MenuItem)sender).IsChecked = true;
                MainWindow.rightTabTargetColumn.Width = new GridLength(2, GridUnitType.Star);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MenuItem item = ((MenuItem)sender);
            if (item.IsChecked)
            {
                ((MenuItem)sender).IsChecked = false;
                MainWindow.topTabTargetRow.Height = new GridLength(0, GridUnitType.Star);
            }
            else
            {
                ((MenuItem)sender).IsChecked = true;
                MainWindow.topTabTargetRow.Height = new GridLength(4, GridUnitType.Star);
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            MenuItem item = ((MenuItem)sender);
            if (item.IsChecked)
            {
                ((MenuItem)sender).IsChecked = false;
                APLan.Views.Draw.signalresultsRow.Height = new GridLength(0, GridUnitType.Star);
            }
            else
            {
                ((MenuItem)sender).IsChecked = true;
                APLan.Views.Draw.signalresultsRow.Height = new GridLength(15, GridUnitType.Star);
            }
        }
    }
}
