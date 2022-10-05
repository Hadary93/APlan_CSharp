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
    /// Interaction logic for VisualizedData.xaml
    /// </summary>
    public partial class VisualizedData : UserControl
    {
        public VisualizedData()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //if (APLan.Views.Draw.myKantenLines.ItemsSource == null)
            //{
            //    //Draw.myKantenLines.ItemsSource = APLan.App.Current.FindResource("newProjectViewModel");
            //}
            //;
        }
    }
}
