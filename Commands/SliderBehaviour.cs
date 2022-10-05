using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APLan.Commands
{
    public class SliderBehaviour
    {
        #region Slider
        public static readonly DependencyProperty SliderCommandProperty =
            DependencyProperty.RegisterAttached("SliderCommand", typeof(ICommand), typeof(SliderBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(SliderCommandChanged)));
        private static void SliderCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider element = (Slider)d;

            element.ValueChanged += Element_ValueChanged;
        }
        static void Element_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider element = (Slider)sender;

            ICommand command = GetSliderCommand(element);

            command.Execute(e);
        }
        public static void SetSliderCommand(UIElement element, ICommand value)
        {
            element.SetValue(SliderCommandProperty, value);
        }
        public static ICommand GetSliderCommand(UIElement element)
        {
            return (ICommand)element.GetValue(SliderCommandProperty);
        }
        #endregion
        #region ItemRotateSlider
        public static readonly DependencyProperty ItemRotateSliderCommandProperty =
            DependencyProperty.RegisterAttached("ItemRotateSliderCommand", typeof(ICommand), typeof(SliderBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(ItemRotateSliderCommandChanged)));
        private static void ItemRotateSliderCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider element = (Slider)d;

            element.ValueChanged += element_ItemRotateValueChanged;
        }
        static void element_ItemRotateValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider element = (Slider)sender;

            ICommand command = GetItemRotateSliderCommand(element);

            command.Execute(e);
        }
        public static void SetItemRotateSliderCommand(UIElement element, ICommand value)
        {
            element.SetValue(ItemRotateSliderCommandProperty, value);
        }
        public static ICommand GetItemRotateSliderCommand(UIElement element)
        {
            return (ICommand)element.GetValue(ItemRotateSliderCommandProperty);
        }
        #endregion
        #region ItemScaleSlider
        public static readonly DependencyProperty ItemScaleSliderCommandProperty =
            DependencyProperty.RegisterAttached("ItemScaleSliderCommand", typeof(ICommand), typeof(SliderBehaviour), new FrameworkPropertyMetadata(new PropertyChangedCallback(ItemScaleSliderCommandChanged)));
        private static void ItemScaleSliderCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Slider element = (Slider)d;

            element.ValueChanged += Element_ItemScaleValueChanged;
        }
        static void Element_ItemScaleValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider element = (Slider)sender;

            ICommand command = GetItemScaleSliderCommand(element);

            command.Execute(e);
        }
        public static void SetItemScaleSliderCommand(UIElement element, ICommand value)
        {
            element.SetValue(ItemScaleSliderCommandProperty, value);
        }
        public static ICommand GetItemScaleSliderCommand(UIElement element)
        {
            return (ICommand)element.GetValue(ItemScaleSliderCommandProperty);
        }
        #endregion
    }
}
