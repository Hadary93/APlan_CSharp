using APLan.Commands;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using APLan.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using APLan.HelperClasses;

namespace APLan.ViewModels
{
    public class CanvasToolsTabViewModel : INotifyPropertyChanged
    {
        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region attributes
        private double previousAngle; // store the current rotation of the canvas.
        private Brush _selectBrush;
        private Brush _moveBrush;
        private Brush _dragBrush;
        public Brush SelectBrush
        {
            get
            {
                return _selectBrush;
            }

            set
            {
                _selectBrush = value;
                OnPropertyChanged("SelectBrush");
            }
        }
        public Brush MoveBrush
        {
            get
            {
                return _moveBrush;
            }

            set
            {
                _moveBrush = value;
                OnPropertyChanged("MoveBrush");
            }
        }
        public Brush DragBrush
        {
            get
            {
                return _dragBrush;
            }

            set
            {
                _dragBrush = value;
                OnPropertyChanged("DragBrush");
            }
        }
        #endregion

        #region commands
        public ICommand SelectButton { get; set; }
        public ICommand MoveButton { get; set; }
        public ICommand DragButton { get; set; }
        private ICommand rotateCanvasSlider { get; set; }
        public ICommand RotateCanvasSlider
        {
            get
            {
                return rotateCanvasSlider ??= new RelayCommand(
                   x =>
                   {
                       ExecuteCanvasSliderChange((RoutedPropertyChangedEventArgs<double>)x);
                   });
            }
        }
        private RelayCommand _mouseDownCommand;
        public RelayCommand MouseDownCommand
        {
            get
            {
                if (_mouseDownCommand == null) return _mouseDownCommand = new RelayCommand(param => ExecuteMouseDown((MouseEventArgs)param));
                return _mouseDownCommand;
            }
            set { _mouseDownCommand = value; }
        }

        #endregion

        #region constructor
        public CanvasToolsTabViewModel()
        {
            MoveButton = new RelayCommand(ExecuteMoveButton);
            DragButton = new RelayCommand(ExecuteDragButton);
            SelectButton = new RelayCommand(ExecuteSelectButton);
            SelectBrush = Brushes.White;
            MoveBrush = Brushes.White;
            DragBrush = Brushes.White;
        }
        #endregion
        
        #region logic 
        /// <summary>
        /// rotate the canvas.
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteCanvasSliderChange(RoutedPropertyChangedEventArgs<double> e)
        {

            //rotate around midpoint of drawing visible area.
            Point p = new((Draw.drawingScrollViewer.ActualWidth / 2), ((Draw.drawingScrollViewer.ActualWidth / 2)));
            Point translatedP = Draw.drawingScrollViewer.TranslatePoint(p, Draw.drawing);

            MatrixTransform transform = (MatrixTransform)Draw.drawing.RenderTransform;
            Matrix matrix = transform.Matrix;
            matrix.RotateAtPrepend(-previousAngle, translatedP.X, translatedP.Y);
            matrix.RotateAtPrepend(e.NewValue, translatedP.X, translatedP.Y);

            //update previous applied angle for next time use.
            previousAngle = e.NewValue;
            transform.Matrix = matrix;
        }
        /// <summary>
        /// allow selection
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteSelectButton(object parameter)
        {
            if (DrawViewModel.tool != DrawViewModel.SelectedTool.Select)
            {
                DrawViewModel.tool = DrawViewModel.SelectedTool.Select;
                SelectBrush = Brushes.Gray;
                MoveBrush = Brushes.White;
                DragBrush = Brushes.White;
            }
            else if (DrawViewModel.tool == DrawViewModel.SelectedTool.Select)
            {
                DrawViewModel.tool = DrawViewModel.SelectedTool.None;
                SelectBrush = Brushes.White;
            }

        }
        /// <summary>
        /// allow moving
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteMoveButton(object parameter)
        {
            if (DrawViewModel.tool != DrawViewModel.SelectedTool.Move)
            {
                DrawViewModel.tool = DrawViewModel.SelectedTool.Move;
                SelectBrush = Brushes.White;
                MoveBrush = Brushes.Gray;
                DragBrush = Brushes.White;
            }
            else if (DrawViewModel.tool == DrawViewModel.SelectedTool.Move)
            {
                DrawViewModel.tool = DrawViewModel.SelectedTool.None;
                MoveBrush = Brushes.White;
            }

        }
        /// <summary>
        /// allow dragging
        /// </summary>
        /// <param name="parameter"></param>
        public void ExecuteDragButton(object parameter)
        {
            if (DrawViewModel.tool != DrawViewModel.SelectedTool.Drag)
            {
                DrawViewModel.tool = DrawViewModel.SelectedTool.Drag;
                SelectBrush = Brushes.White;
                MoveBrush = Brushes.White;
                DragBrush = Brushes.Gray;
            }
            else if (DrawViewModel.tool == DrawViewModel.SelectedTool.Drag)
            {
                DrawViewModel.tool = DrawViewModel.SelectedTool.None;
                DragBrush = Brushes.White;
            }
        }
        /// <summary>
        /// allow draging for a text
        /// </summary>
        /// <param name="e"></param>
        private void ExecuteMouseDown(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CustomCanvasText textBox = new();
                DragDrop.DoDragDrop(textBox, new DataObject(DataFormats.Serializable, textBox), DragDropEffects.Move);
            }
        }

        #endregion
    }
}
