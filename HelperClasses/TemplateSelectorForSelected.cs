using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace APLan.HelperClasses
{
    /// <summary>
    /// select the template to represent the information about the selected items from the Canvas.
    /// </summary>
    public class TemplateSelectorForSelected : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                if (item.GetType() == typeof(Path))
                    return
                       Application.Current.FindResource("CustomPolylineTempalte") as DataTemplate;
                else 
                    return
                       Application.Current.FindResource("SymbolTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
