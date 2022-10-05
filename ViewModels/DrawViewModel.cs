using APLan.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using APLan.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using APLan.HelperClasses;

namespace APLan.ViewModels
{
    public class DrawViewModel : INotifyPropertyChanged
    {
        #region Inotify essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
        
        #region enum
        public enum SelectedTool
        {
            None,
            Select,
            Drag,
            Move,
            Rotate,
            Scale
        }
        #endregion

        #region attributes
        public static ModelViewModel model;

        //canvas data
        private double _rotateItems = 0;
        private double canvasScale = 1;
        private double gridThicnkess = 0.5;
        private double lineThicnkess = 2;
        private double canvasSize = 100000;
        private double signalSize;
        public static double sharedCanvasSize = 100000; //this should be always equal canvasSize.
        public static double drawingScale = 1;
        public static double signalSizeForConverter;
        public static Point GlobalDrawingPoint = new Point(0, 0);

        //for mouse location.
        private System.Windows.Point pointer;
        //mouse location info.
        private string _xLocation = String.Empty;
        private string _yLocation = String.Empty;
        //multiselection rectangle info.
        Rectangle selectionRectangle = null;
        public System.Windows.Point firsPoint;
        Point InitialMovePoint;
        Point InitialDragPoint;
        Point MoveOffest;
        Point DraOffset;

        public ArrayList Multiselected;
        public ArrayList tempSelected;

        public static SelectedTool tool;

        public static List<UIElement> toBeStored
        {
            get;
            set;
        }
        public ObservableCollection<UIElement> selected
        {
            get;
            set;
        }

        public double CanvasSize
        {
            get => canvasSize;
        }
        public double CanvasScale
        {
            get => canvasScale;
            set => canvasScale = canvasScale * value;
        }
        public double GridThicnkess
        {
            get
            {
                if (gridThicnkess <= 0.5)
                {
                    return gridThicnkess;
                }
                return 0.5;
                // return gridThicnkess;
            }
            set
            {
                gridThicnkess = (1 / value) * 0.5;
                OnPropertyChanged("GridThicnkess");
            }
        }
        public double LineThicnkess
        {
            get
            {
                if (lineThicnkess <= 2)
                {
                    return lineThicnkess;
                }
                return 2;
            }

            set
            {

                lineThicnkess = (1 / value) * 2;
                OnPropertyChanged("LineThicnkess");
            }
        }
        public double RotateTextBox
        {
            get => _rotateItems;
            set
            {
                _rotateItems = value;
                OnPropertyChanged("RotateTextBox");
            }
        }
        public double ScaleTextBox
        {
            get => _rotateItems;
            set
            {
                _rotateItems = value;
                OnPropertyChanged("RotateTextBox");
            }
        }
        public double SignalSize
        {
            get => signalSize; set
            {
                signalSize = value;
                signalSizeForConverter = value;
            }
        }

        public string Xlocation
        {
            get => _xLocation;
            set
            {
                if (_xLocation != value)
                {
                    _xLocation = value;
                    OnPropertyChanged(); // reports this property
                }
            }
        }
        public string Ylocation
        {
            get => _yLocation;
            set
            {
                if (_yLocation != value)
                {
                    _yLocation = value;
                    OnPropertyChanged(); // reports this property
                }
            }
        }

        #endregion

        #region constructor
        public DrawViewModel()
        {
            //store and load.
            toBeStored = new List<UIElement>();
            

            selected = new ObservableCollection<UIElement>();
            Multiselected = new ArrayList();
            tempSelected = new ArrayList();

            tool = SelectedTool.None;
            pointer = new Point(double.PositiveInfinity, double.PositiveInfinity);
            firsPoint = new Point(0, 0);
            InitialMovePoint = new Point(-1, -1);
            InitialDragPoint = new Point(-1, -1);
            MoveOffest = new Point(-1, -1);
            DraOffset = new Point(-1, -1);
            
            SignalSize = 10;

            RotateSelectionButton = new RelayCommand(rotateSelection);
            
            

        }
        #endregion

        #region commands
        private ICommand _MouseleftButtonDownCommand;
        private ICommand _MouserightButtonDownCommand;
        private ICommand _MouseMiddleDownCommand;
        private ICommand _MouseWheelCommand;
        private ICommand _DrawingMouseMoveCommand;
        private ICommand _BasCanvasMouseMoveCommand;
        private ICommand _KeyDownForMainWindow;
        private ICommand _ObjectLodaded;
        private ICommand _RotateItemSlider { get; set; }
        private ICommand _ScaleItemSlider { get; set; }
        public ICommand RotateSelectionButton { get; set; }

        
        public ICommand KeyDownForMainWindow
        {
            get
            {
                return _KeyDownForMainWindow ??= new RelayCommand(
                   x =>
                   {
                       ExecuteKeyDownPressed((KeyEventArgs)x);
                   });
            }
        }
        public ICommand LeftMouseButtonDown
        {
            get
            {
                return _MouseleftButtonDownCommand ??= new RelayCommand(
                   x =>
                   {
                       ExecuteMouseLeftButtonDownDrawingCanvas((MouseEventArgs)x);
                   });
            }
        }
        public ICommand MouseMiddleDownCommand
        {
            get
            {
                return _MouseMiddleDownCommand ??= new RelayCommand(
                   x =>
                   {
                       MessageBox.Show("MiddelPressed");
                   });
            }
        }
        public ICommand RightMouseButtonDown
        {
            get
            {
                return _MouserightButtonDownCommand ??= new RelayCommand(
                   x =>
                   {
                       MessageBox.Show("RightPressed");
                   });
            }
        }
        public ICommand MouseWheelCommand
        {
            get
            {
                return _MouseWheelCommand ??= new RelayCommand(
                   x =>
                   {
                       ExecuteMouseWheelDrawingCanvas((MouseWheelEventArgs)x);
                   });
            }
        }
        public ICommand DrawingMouseMoveCommand
        {
            get
            {
                return _DrawingMouseMoveCommand ??= new RelayCommand(
                   x =>
                   {
                       ExecuteMouseMoveDrawingCanvas((MouseEventArgs)x);
                   });
            }
        }
        public ICommand BasCanvasMouseMoveCommand
        {
            get
            {
                return _BasCanvasMouseMoveCommand ??= new RelayCommand(
                   x =>
                   {
                       ExecuteMouseMoveBaseCanvas((MouseEventArgs) x);
                   });
            }
        }
        public ICommand ObjectLoaded
        {
            get
            {
                return _ObjectLodaded ??= new RelayCommand(
                   x =>
                   {
                       ExecuteLoadObjects((UIElement) x);
                   });
            }
        }
        public ICommand RotateItemSlider
        {
            get
            {
                return _RotateItemSlider ??= new RelayCommand(
                   x =>
                   {
                       ExecuteRotateItemSliderChange((RoutedPropertyChangedEventArgs<double>)x);
                   });
            }
        }
        public ICommand ScaleItemSlider
        {
            get
            {
                return _ScaleItemSlider ??= new RelayCommand(
                   x =>
                   {
                       ExecuteScaleItemSliderChange((RoutedPropertyChangedEventArgs<double>)x);
                   });
            }
        }


        #endregion
 
        #region mouse events logic
        /// <summary>
        /// logic whenever the mouse is moving. applied on the base canvas for hitTesting.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteMouseMoveBaseCanvas(MouseEventArgs e)
        {
            if (tool==SelectedTool.Select)
            {
                multiselectAlgo(e); // apply multiselection while the mouse if moving if selection is allowed.
            }
            
        }
        /// <summary>
        /// logic applied to the drawing canvas.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteMouseMoveDrawingCanvas(MouseEventArgs e)
        {
            // getting the desired canvas as the event is applied on several objects not on source only.
            Canvas hostCanvas = null;

            if (e.Source.GetType() == typeof(Canvas))
            {
                hostCanvas = (Canvas)((Canvas)e.Source).Parent;
            }
            else
            {
                hostCanvas = (Canvas)(LogicalTreeHelper.GetParent((FrameworkElement)e.Source));
                hostCanvas = (Canvas)hostCanvas.Parent;
            }

            ///////////////////////////////////////////////////logic section//////////////////////////////////////
            //update the mouse coordinates.

            //Xlocation = ((e.GetPosition(hostCanvas.Children[0]).X - canvasSize / 2)*(1/drawingScale) +GlobalDrawingPoint.X).ToString();
            //Ylocation = ((-e.GetPosition(hostCanvas.Children[0]).Y + canvasSize / 2) * (1/drawingScale) + GlobalDrawingPoint.Y).ToString();
            Xlocation = (e.GetPosition(hostCanvas.Children[0]).X).ToString();
            Ylocation = (e.GetPosition(hostCanvas.Children[0]).Y).ToString();




            //dragging
            if (tool == SelectedTool.Drag && e.LeftButton == MouseButtonState.Pressed)
            {
                dragSelection(e);
            }

            // canvas drag
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                
                if (pointer.X == double.PositiveInfinity)
                {
                    pointer = Mouse.GetPosition(hostCanvas);              
                }
                double diff_x = Mouse.GetPosition(hostCanvas).X - pointer.X;
                double diff_y = Mouse.GetPosition(hostCanvas).Y - pointer.Y;
                Canvas.SetLeft(hostCanvas, Canvas.GetLeft(hostCanvas) + diff_x);
                
                Canvas.SetTop(hostCanvas, Canvas.GetTop(hostCanvas) + diff_y);

            }
            if (e.MiddleButton == MouseButtonState.Released && e.Source.GetType() == typeof(Canvas))
            {
                pointer = new Point(double.PositiveInfinity, double.PositiveInfinity);
            }
            if (e.LeftButton == MouseButtonState.Released)
            {
                InitialDragPoint = new Point(-1, -1);
            }
        }
        /// <summary>
        /// apply the mouseWheel action on the drawing canvas.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteMouseWheelDrawingCanvas(MouseWheelEventArgs e)
        {

            // getting the desired canvas as the event is applied not only on source object.
            Canvas drawingCanvas = null;

            if (e.Source.GetType() == typeof(Canvas))
            {
                drawingCanvas = ((Canvas)e.Source);
            }
            else
            {
                drawingCanvas = (Canvas)(LogicalTreeHelper.GetParent((FrameworkElement)e.Source));
            }


            ///////////////////////////////////////////////logic/////////////////////////
            var element = drawingCanvas;
            var position = e.GetPosition(element);
            var transform = (MatrixTransform)element.RenderTransform;
            var matrix = transform.Matrix;
            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1);
            CanvasScale = scale;
            GridThicnkess = CanvasScale;
            LineThicnkess = CanvasScale;
            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            transform.Matrix = matrix;
               
        }
        /// <summary>
        /// apply leftMouseDown on the drawing canvas.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteMouseLeftButtonDownDrawingCanvas(MouseEventArgs e)
        {
            switch (tool)
            {
                case SelectedTool.Move:
                    moveSelection(e); //moving the selected items
                    break;
                case SelectedTool.Select:
                    singleSelection(e); // select an item.
                    break;
            }
        }
        #endregion
        
        #region Key logic
        /// <summary>
        /// apply keydown on the main window.
        /// </summary>
        /// <param name="e"></param>
        public void ExecuteKeyDownPressed(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    deleteSelected();
                    break;
                case Key.Escape:
                    CancelSelected();
                    break;
            }
        }
        #endregion
        
        #region button logic
        /// <summary>
        /// rotate the selected items from the canvas by a specific value.
        /// </summary>
        /// <param name="parameter"></param>
        private void rotateSelection(object parameter)
        {
            foreach (UIElement element in selected)
            {
                if (element.GetType() != typeof(System.Windows.Shapes.Path))
                {
                    //update this part when we have different objects than image.
                    Matrix newMatrix = new Matrix();
                    newMatrix.RotatePrepend(RotateTextBox - CustomProperites.GetRotation(element));

                    Matrix oldMatrix = element.RenderTransform.Value;

                    MatrixTransform m = new MatrixTransform();

                    m.Matrix = newMatrix * oldMatrix;
                    element.RenderTransformOrigin = new Point(0.5, 0.5);
                    element.RenderTransform = m;


                    //Old rotation
                    CustomProperites.SetRotation(element, RotateTextBox);
                }
            }
        }
        /// <summary>
        /// rotate the selected items by a slider.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteRotateItemSliderChange(RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (UIElement element in selected)
            {
                if (element.GetType() != typeof(System.Windows.Shapes.Path) && element.GetType() != typeof(APLan.HelperClasses.CustomSignal))
                {
                    //update this part when we have different objects than image.
                    Matrix newMatrix = new Matrix();
                    newMatrix.RotatePrepend(e.NewValue - CustomProperites.GetRotation(element));

                    Matrix oldMatrix = element.RenderTransform.Value;

                    MatrixTransform m = new MatrixTransform();

                    m.Matrix = newMatrix * oldMatrix;
                    if (element is Image)
                    {
                        element.RenderTransformOrigin = new Point(0.5, 0.5);
                    }else if (element is TextBox)
                    {
                        element.RenderTransformOrigin = new Point(0, 0);
                    }
                    
                    element.RenderTransform = m;


                    //Old rotation
                    CustomProperites.SetRotation(element, e.NewValue);
                }
            }
        }
        /// <summary>
        /// scale selected items by a slider.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteScaleItemSliderChange(RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (UIElement element in selected)
            {
                if (element.GetType() != typeof(System.Windows.Shapes.Path) && element.GetType() != typeof(APLan.HelperClasses.CustomSignal))
                {
                    //update this part when we have different objects than image.
                    Matrix newMatrix = new Matrix();
                    newMatrix.Scale(1/CustomProperites.GetScale(element), 1/CustomProperites.GetScale(element));
                    newMatrix.Scale(e.NewValue, e.NewValue);

                    Matrix oldMatrix = element.RenderTransform.Value;

                    MatrixTransform m = new MatrixTransform();

                    m.Matrix = newMatrix * oldMatrix;
                    element.RenderTransformOrigin = new Point(0.5, 0.5);
                    element.RenderTransform = m;


                    //Old rotation
                    CustomProperites.SetScale(element, e.NewValue);
                }
            }
        }
        
        #endregion
        
        #region additional logic
        /// <summary>
        /// algorithm to select multiple items at once.
        /// </summary>
        /// <param name="e"></param>
        private void multiselectAlgo( MouseEventArgs e)
        {
            Draw.baseToDraw.Children.Remove(selectionRectangle);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                selectionRectangle = new Rectangle() { Fill = Brushes.LightSkyBlue, Stroke = Brushes.Black, Opacity = 0.5 , IsHitTestVisible=false};
                Draw.baseToDraw.Children.Add(selectionRectangle);

                var mouseX = Mouse.GetPosition(Draw.baseToDraw).X;
                var mouseY = Mouse.GetPosition(Draw.baseToDraw).Y;

                if (firsPoint.X == 0)
                {
                    firsPoint = Mouse.GetPosition(Draw.baseToDraw);

                }
                else
                {
                    selectionRectangle.Width = Math.Abs(mouseX - firsPoint.X);
                    selectionRectangle.Height = Math.Abs(mouseY - firsPoint.Y);
                    Canvas.SetLeft(selectionRectangle, firsPoint.X);
                    Canvas.SetTop(selectionRectangle, firsPoint.Y);
                    ScaleTransform s = new ScaleTransform();
                    RectangleGeometry expandedHitTestArea = new RectangleGeometry(new Rect(Canvas.GetLeft(selectionRectangle), Canvas.GetTop(selectionRectangle), selectionRectangle.Width, selectionRectangle.Height));
                    if (mouseX - firsPoint.X >= 0 && mouseY - firsPoint.Y < 0)
                    {
                        s.ScaleX = 1;
                        s.ScaleY = -1;
                        selectionRectangle.RenderTransform = s;
                        expandedHitTestArea = new RectangleGeometry(new Rect(Canvas.GetLeft(selectionRectangle), Canvas.GetTop(selectionRectangle) - selectionRectangle.Height, selectionRectangle.Width, selectionRectangle.Height));

                    }
                    else if (mouseX - firsPoint.X <= 0 && mouseY - firsPoint.Y < 0)
                    {
                        s.ScaleX = -1;
                        s.ScaleY = -1;
                        selectionRectangle.RenderTransform = s;
                        expandedHitTestArea = new RectangleGeometry(new Rect(Canvas.GetLeft(selectionRectangle) - selectionRectangle.Width, Canvas.GetTop(selectionRectangle) - selectionRectangle.Height, selectionRectangle.Width, selectionRectangle.Height));

                    }
                    else if (mouseX - firsPoint.X <= 0 && mouseY - firsPoint.Y > 0)
                    {
                        s.ScaleX = -1;
                        s.ScaleY = 1;
                        selectionRectangle.RenderTransform = s;
                        expandedHitTestArea = new RectangleGeometry(new Rect(Canvas.GetLeft(selectionRectangle) - selectionRectangle.Width, Canvas.GetTop(selectionRectangle), selectionRectangle.Width, selectionRectangle.Height));
                    }
                    multiSelectHitTest(expandedHitTestArea);
                }
            }
            if (e.LeftButton == MouseButtonState.Released)
            {
                firsPoint = new Point(0, 0);
                foreach (UIElement ee in Multiselected)
                {
                    if (selected.Contains(ee) == false)
                    {
                        selected.Add(ee);
                        ee.Opacity = 0.5;
                    }
                }
                Multiselected.Clear();
            }
        }
        /// <summary>
        /// hit testing by a rectanlge for multiselection.
        /// </summary>
        /// <param name="expandedHitTestArea"></param>
        private void multiSelectHitTest(RectangleGeometry expandedHitTestArea)
        {
            //here the selection is UIElement we need to adabt for other objects
            foreach (UIElement e in tempSelected)
            {
                

                Multiselected.Remove(e);
                if (selected.Contains(e) == false)
                {
                    
                    e.Opacity = 1;
                }
            }
            tempSelected.Clear();
            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(Draw.baseToDraw,
                new HitTestFilterCallback(MyHitTestFilter),
                new HitTestResultCallback(MultiSelectionHitTestResult),
                new GeometryHitTestParameters(expandedHitTestArea));

            // Perform actions on the hit test results list.
            if (tempSelected.Count > 0)
            {
                
                ProcessMultiSelectHitTestResultsList();
            }
        }
        /// <summary>
        /// select a single item.
        /// </summary>
        /// <param name="e"></param>
        private void singleSelection(MouseEventArgs e)
        {

            if (e.Source.GetType() != typeof(Canvas))
            {
                Canvas c = (Canvas)VisualTreeHelper.GetParent((DependencyObject)e.Source);
                Point pt = e.GetPosition(c);
                // Perform the hit test against a given portion of the visual object tree.
                HitTestResult result = VisualTreeHelper.HitTest(c, pt);

                if (result != null && ((UIElement)result.VisualHit).IsVisible == true)
                {
                    UIElement element = (UIElement)result.VisualHit;


                    if (selected.Contains(element) == false)
                    {
                        element.Opacity = 0.5;
                        selected.Add(element);

                    }
                    else
                    {
                        element.Opacity = 1;
                        selected.Remove(element);

                    }
                }
            }

        }
        /// <summary>
        /// move all selected items by two clicks.
        /// </summary>
        /// <param name="e"></param>
        private void moveSelection(MouseEventArgs e)
        {
            Canvas drawingCanvas = null;
            if (e.Source.GetType() != typeof(Canvas))
            {
                drawingCanvas = (Canvas)VisualTreeHelper.GetParent((DependencyObject)e.Source);
            }
            else
            {
                drawingCanvas = (Canvas)e.Source;
            }


            if (InitialMovePoint.X == -1)
            {
                InitialMovePoint = Mouse.GetPosition(drawingCanvas);

            }
            else if (InitialMovePoint.X != -1)
            {
                MoveOffest.X = Mouse.GetPosition(drawingCanvas).X - InitialMovePoint.X;
                MoveOffest.Y = Mouse.GetPosition(drawingCanvas).Y - InitialMovePoint.Y;
                InitialMovePoint = new Point(-1, -1);
                updateSelectionMoveOperation(MoveOffest.X, MoveOffest.Y);
            }
        }
        /// <summary>
        /// drag all selected items.
        /// </summary>
        /// <param name="e"></param>
        private void dragSelection(MouseEventArgs e)
        {
            Canvas drawingCanvas = null;
            if (e.Source.GetType() != typeof(Canvas))
            {
                drawingCanvas = (Canvas)VisualTreeHelper.GetParent((DependencyObject)e.Source);
            }
            else
            {
                drawingCanvas = (Canvas)e.Source;
            }

            if (InitialDragPoint.X == -1)
            {
                InitialDragPoint = Mouse.GetPosition(drawingCanvas);
            }
            DraOffset.X = Mouse.GetPosition(drawingCanvas).X - InitialDragPoint.X;
            DraOffset.Y = Mouse.GetPosition(drawingCanvas).Y - InitialDragPoint.Y;
            updateSelectionMoveOperation(DraOffset.X, DraOffset.Y);
            InitialDragPoint = Mouse.GetPosition(drawingCanvas);

        }
        /// <summary>
        /// update information of objects whenever they are moved.
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        private void updateSelectionMoveOperation(double offsetX, double offsetY)
        {
            //for now it is for UIElements only wee need to update whenever other types are introduced.
            foreach (UIElement element in selected)
            {
                if (element.GetType() != typeof(System.Windows.Shapes.Path) && element.GetType() != typeof(APLan.HelperClasses.CustomSignal))
                {
                    Matrix newMatrix = new Matrix();
                    newMatrix.Translate(offsetX, offsetY);
                    Matrix oldMatrix = element.RenderTransform.Value;
                    MatrixTransform m = new MatrixTransform();
                    m.Matrix = oldMatrix * newMatrix;
                    element.RenderTransform = m;
                }

            }
        }
        /// <summary>
        /// delet selected items.
        /// </summary>
        public void deleteSelected()
        {

            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i].GetType() != typeof(System.Windows.Shapes.Path) && selected[i].GetType() != typeof(APLan.HelperClasses.CustomSignal))
                {

                    Canvas c = (Canvas)LogicalTreeHelper.GetParent(selected[i]);
                    if (c != null && c.Children.Contains(selected[i]))
                    {
                        c.Children.Remove(selected[i]);
                        toBeStored.Remove((UIElement)selected[i]);
                        selected.Remove(selected[i]);
                    }
                    else // it is a loaded object
                    {
                        if (selected[i] is CustomCanvasSignal)
                        {
                            ((ObservableCollection<CanvasObjectInformation>)findItemControlParent(selected[i]).ItemsSource).Remove((CanvasObjectInformation)((CustomCanvasSignal)selected[i]).DataContext);
                            // loadedObjects.Remove((CanvasObjectInformation)((CustomCanvasSignal)selected[i]).DataContext);
                        }
                        else if (selected[i] is CustomCanvasText)
                        {
                            ((ObservableCollection<CanvasObjectInformation>)findItemControlParent(selected[i]).ItemsSource).Remove((CanvasObjectInformation)((CustomCanvasText)selected[i]).DataContext);
                            // loadedObjects.Remove((CanvasObjectInformation)((CustomCanvasText)selected[i]).DataContext);
                            toBeStored.Remove(selected[i]);
                        }

                    }
                }
            }
        }
        /// <summary>
        /// cancel any selection.
        /// </summary>
        public void CancelSelected()
        {
            foreach (UIElement e in selected)
            {
                e.Opacity = 1;
            }
            selected.Clear();
        }
        /// <summary>
        /// filtering the selection for a specific objects only.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            
            // Test for the object value you want to filter.
            if (o.GetType() == typeof(CustomCanvasText))
            {
                tempSelected.Add(o);
                // Visual object and descendants are NOT part of hit test results enumeration.
                return HitTestFilterBehavior.Continue;
            }
            else
            {
                // Visual object is part of hit test results enumeration.
                return HitTestFilterBehavior.Continue;
            }
        }
        /// <summary>
        /// logic applied on the selected items after hitTest.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public HitTestResultBehavior MultiSelectionHitTestResult(HitTestResult result)
        {
            //MessageBox.Show(result.VisualHit.GetType().ToString());
            // Test for the object value you want to filter.
            if (result.VisualHit.GetType() == typeof(CustomCanvasSignal) || result.VisualHit.GetType() == typeof(System.Windows.Shapes.Path))
            {
                //MessageBox.Show(result.VisualHit.GetType().ToString());
                tempSelected.Add(result.VisualHit);
                // Visual object and descendants are NOT part of hit test results enumeration.
                return HitTestResultBehavior.Continue;
            }
            else
            {
                // Visual object is part of hit test results enumeration.
                return HitTestResultBehavior.Continue;
            }
            // Set the behavior to return visuals at all z-order levels.
            //return HitTestResultBehavior.Continue;
        }
        /// <summary>
        /// sub-logic for multiselection.
        /// </summary>
        void ProcessMultiSelectHitTestResultsList()
        {
            foreach (UIElement e in tempSelected)
            {
                
                if (e.GetType() != typeof(Canvas)&& e.IsVisible==true && selected.Contains(e) == false)
                {
                    e.Opacity = 0.5;
                    Multiselected.Add(e); 
                }
            }
        }
        /// <summary>
        /// find the itemControl that contains a selection to delete the objects from the corresponding bounded list.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public ItemsControl findItemControlParent(UIElement e)
        {
            ItemsControl c = null;
            e = (UIElement)VisualTreeHelper.GetParent(e);
            while (e!=null)
            {
                //if (e is Canvas && !Double.IsNaN(((Canvas)e).Width))
                //{
                //    return (Canvas)e;
                //}
                e = (UIElement)VisualTreeHelper.GetParent(e);
                
                if (e is ItemsControl)
                {
                    return (ItemsControl)e;
                    //((ItemsControl)e).ItemsSource = null;
                    // ((ObservableCollection<CanvasObjectInformation>)((ItemsControl)e).ItemsSource).Remove((CanvasObjectInformation)temp.DataContext);
                }
            }
            return c;
        }
        /// <summary>
        /// add the loaded objects to the Canvas for future manipulation.
        /// </summary>
        /// <param name="e"></param>
        public void ExecuteLoadObjects(UIElement e)
        {
            toBeStored.Add(e);
            Canvas.SetLeft(e, 0);
            Canvas.SetTop(e, 0);
            //canvas.Children.Add(e);
        }
        #endregion

    }
}
