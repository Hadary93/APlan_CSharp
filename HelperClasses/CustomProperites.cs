using System.Windows;

namespace APLan
{
    /// <summary>
    /// additional properties for the UIElements that don't have these attributes to help in transformation
    /// </summary>
    public class CustomProperites : DependencyObject
    {

        //applied scale
        public static readonly DependencyProperty ScaleProperty =
         DependencyProperty.RegisterAttached(
         "Scale",
         typeof(double),
         typeof(CustomProperites),
         new FrameworkPropertyMetadata(0.0)
        );
       
        // Declare a get accessor method.
        public static double GetScale(DependencyObject target) =>
            (double)target.GetValue(ScaleProperty);
       
        // Declare a set accessor method.
        public static void SetScale(DependencyObject target, double value) =>
            target.SetValue(ScaleProperty, value);
       
        //applied rotation.
        public static readonly DependencyProperty RotationProperty =
         DependencyProperty.RegisterAttached(
         "Rotation",
         typeof(double),
         typeof(CustomProperites),
         new FrameworkPropertyMetadata(0.0)
        );


        // Declare a get accessor method.
        public static double GetRotation(DependencyObject target) =>
            (double)target.GetValue(RotationProperty);

        // Declare a set accessor method.
        public static void SetRotation(DependencyObject target, double value) =>
            target.SetValue(RotationProperty, value);


        //applied ImageURL.
        public static readonly DependencyProperty ImageURLProperty =
         DependencyProperty.RegisterAttached(
         "ImageURL",
         typeof(string),
         typeof(CustomProperites),
         new FrameworkPropertyMetadata("")
        );


        // Declare a get accessor method.
        public static string GetImageURL(DependencyObject target) =>
            (string)target.GetValue(ImageURLProperty);

        // Declare a set accessor method.
        public static void SetImageURL(DependencyObject target, string value) =>
            target.SetValue(ImageURLProperty, value);


        //applied Location.
        public static readonly DependencyProperty CanvasLocationProperty =
         DependencyProperty.RegisterAttached(
         "CanvasLocation",
         typeof(Point),
         typeof(CustomProperites),
         new FrameworkPropertyMetadata(new Point())
        );


        // Declare a get accessor method.
        public static Point GetLocation(DependencyObject target) =>
            (Point)target.GetValue(CanvasLocationProperty);

        // Declare a set accessor method.
        public static void SetLocation(DependencyObject target, Point value) =>
            target.SetValue(CanvasLocationProperty, value);


    }
}
