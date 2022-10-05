using System.Windows;
using System.Windows.Controls;

namespace APLan.HelperClasses
{
    /// <summary>
    /// Signal retrieved from the database and visualize in Draw view.
    /// the main goal is to make it non adjustable.
    /// </summary>
    public class CustomSignal : Image
    {
        //applied scale
        public static readonly DependencyProperty AdjustProperty =
         DependencyProperty.RegisterAttached(
         "Adjust",
         typeof(bool),
         typeof(CustomSignal),
         new PropertyMetadata(default(bool))
        );

        // Declare a get accessor method.
        public static bool GetAdjust(CustomSignal target) =>
            (bool)target.GetValue(AdjustProperty);

        // Declare a set accessor method.
        public static void SetAdjust(CustomSignal target, bool value) =>
            target.SetValue(AdjustProperty, value);
    }
}
