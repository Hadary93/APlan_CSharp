using System.Windows;
using System.Windows.Controls;

namespace APLan.HelperClasses
{
    /// <summary>
    /// select the loaded template according to the type of the loaded object.
    /// </summary>
    public class TemplateSelectorForLoadedObjects : DataTemplateSelector
    {
        public override DataTemplate
           SelectTemplate(object item, DependencyObject container)
        {

            if (item != null)
            {
                if (((CanvasObjectInformation)item).Type == typeof(CustomCanvasSignal).ToString())
                    return
                       Application.Current.FindResource("loadedSignal") as DataTemplate;
                else if (((CanvasObjectInformation)item).Type == typeof(CustomCanvasText).ToString())
                    return
                       Application.Current.FindResource("loadedText") as DataTemplate;
            }

            return null;
        }
    }
}
