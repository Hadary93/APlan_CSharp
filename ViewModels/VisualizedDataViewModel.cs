using APLan.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APLan.ViewModels
{
    public class VisualizedDataViewModel : INotifyPropertyChanged
    {
        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region attributes

        private double lineThicnkess = 0.5;
        public double LineThicnkess
        {
            get => lineThicnkess;
            set
            {
                lineThicnkess = value;
                OnPropertyChanged();
            }

        }

        //lines and Nodes visibility
        private Visibility _gleisKantenVisibility;
        private Visibility _Entwurfselement_LA_Visibility;
        private Visibility _Entwurfselement_KM_Visibility;
        private Visibility _Entwurfselement_HO_Visibility;
        private Visibility _Entwurfselement_UH_Visibility;
        private Visibility _gleisknotenVisibility;


        //points visibility
        private Visibility _gleisKantenPointsVisibility;
        private Visibility _Entwurfselement_LA_PointsVisibility;
        private Visibility _Entwurfselement_KM_PointsVisibility;
        private Visibility _Entwurfselement_HO_PointsVisibility;
        private Visibility _Entwurfselement_UH_PointsVisibility;

        public Visibility gleisKantenVisibility
        {
            get => _gleisKantenVisibility;
            set
            {
                _gleisKantenVisibility = value;
                OnPropertyChanged("gleisKantenVisibility");
            }

        }
        //horizontal
        public Visibility Entwurfselement_LA_Visibility
        {
            get => _Entwurfselement_LA_Visibility;
            set
            {
                _Entwurfselement_LA_Visibility = value;
                OnPropertyChanged("Entwurfselement_LA_Visibility");
            }
        }
        //meilage
        public Visibility Entwurfselement_KM_Visibility
        {
            get => _Entwurfselement_KM_Visibility;
            set
            {
                _Entwurfselement_KM_Visibility = value;
                OnPropertyChanged("Entwurfselement_KM_Visibility");
            }
        }
        //vertical
        public Visibility Entwurfselement_HO_Visibility
        {
            get => _Entwurfselement_HO_Visibility;
            set
            {
                _Entwurfselement_HO_Visibility = value;
                OnPropertyChanged("Entwurfselement_HO_Visibility");
            }
        }
        //cant
        public Visibility Entwurfselement_UH_Visibility
        {
            get => _Entwurfselement_UH_Visibility;
            set
            {
                _Entwurfselement_UH_Visibility = value;
                OnPropertyChanged("Entwurfselement_UH_Visibility");
            }
        }
        //nodes
        public Visibility gleisknotenVisibility
        {
            get => _gleisknotenVisibility;
            set
            {
                _gleisknotenVisibility = value;
                OnPropertyChanged("gleisknotenVisibility");
            }
        }


        public Visibility GleisKantenPointsVisibility
        {
            get => _gleisKantenPointsVisibility;
            set
            {
                _gleisKantenPointsVisibility = value;
                OnPropertyChanged();
            }

        }
        //horizontal
        public Visibility Entwurfselement_LA_PointsVisibility
        {
            get => _Entwurfselement_LA_PointsVisibility;
            set
            {
                _Entwurfselement_LA_PointsVisibility = value;
                OnPropertyChanged();
            }
        }
        //meilage
        public Visibility Entwurfselement_KM_PointsVisibility
        {
            get => _Entwurfselement_KM_PointsVisibility;
            set
            {
                _Entwurfselement_KM_PointsVisibility = value;
                OnPropertyChanged();
            }
        }
        //vertical
        public Visibility Entwurfselement_HO_PointsVisibility
        {
            get => _Entwurfselement_HO_PointsVisibility;
            set
            {
                _Entwurfselement_HO_PointsVisibility = value;
                OnPropertyChanged();
            }
        }
        //cant
        public Visibility Entwurfselement_UH_PointsVisibility
        {
            get => _Entwurfselement_UH_PointsVisibility;
            set
            {
                _Entwurfselement_UH_PointsVisibility = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        //lines and nodes.
        public ICommand Kanten { get; set; }
        public ICommand Knoten { get; set; }
        public ICommand Horizontal { get; set; }
        public ICommand Vertical { get; set; }
        public ICommand Meilage { get; set; }
        public ICommand Cant { get; set; }

        //points 
        public ICommand KantenPoints { get; set; }
        public ICommand HorizontalPoints { get; set; }
        public ICommand VerticalPoints { get; set; }
        public ICommand MeilagePoints { get; set; }
        public ICommand CantPoints { get; set; }
        #endregion

        #region constructor
        public VisualizedDataViewModel()
        {

            Kanten = new RelayCommand(ExecuteKanten);
            Knoten = new RelayCommand(ExecuteKnoten);
            Horizontal = new RelayCommand(ExecuteHorizontal);
            Vertical = new RelayCommand(ExecuteVertical);
            Meilage = new RelayCommand(ExecuteMeilage);
            Cant = new RelayCommand(ExecuteCant);


            KantenPoints = new RelayCommand(ExecuteKantenPoints);
            HorizontalPoints = new RelayCommand(ExecuteHorizontalPoints);
            VerticalPoints = new RelayCommand(ExecuteVerticalPoints);
            MeilagePoints = new RelayCommand(ExecuteMeilagePoints);
            CantPoints = new RelayCommand(ExecuteCantPoints);


            gleisKantenVisibility = Visibility.Visible;
            Entwurfselement_LA_Visibility = Visibility.Visible;
            Entwurfselement_KM_Visibility = Visibility.Visible;
            Entwurfselement_HO_Visibility = Visibility.Visible;
            Entwurfselement_UH_Visibility = Visibility.Visible;
            gleisknotenVisibility = Visibility.Visible;


            GleisKantenPointsVisibility = Visibility.Collapsed;
            Entwurfselement_LA_PointsVisibility = Visibility.Collapsed;
            Entwurfselement_KM_PointsVisibility = Visibility.Collapsed;
            Entwurfselement_HO_PointsVisibility = Visibility.Collapsed;
            Entwurfselement_UH_PointsVisibility = Visibility.Collapsed;
        }
        #endregion

        #region logic
        public void ExecuteKanten(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (gleisKantenVisibility == Visibility.Visible)
            {
                gleisKantenVisibility = Visibility.Collapsed;
                LineThicnkess = 0;
                box.IsChecked = false;
            }
            else
            {
                gleisKantenVisibility = Visibility.Visible;
                LineThicnkess = 0.5;
                box.IsChecked = true;
                
            }
        }
        public void ExecuteKnoten(object parameter)
        {

            CheckBox box = ((CheckBox)parameter);
            if (gleisknotenVisibility == Visibility.Visible)
            {
                gleisknotenVisibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                gleisknotenVisibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteHorizontal(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_LA_Visibility == Visibility.Visible)
            {
                Entwurfselement_LA_Visibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_LA_Visibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteVertical(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_HO_Visibility == Visibility.Visible)
            {
                Entwurfselement_HO_Visibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_HO_Visibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteMeilage(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_KM_Visibility == Visibility.Visible)
            {
                Entwurfselement_KM_Visibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_KM_Visibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteCant(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_UH_Visibility == Visibility.Visible)
            {
                Entwurfselement_UH_Visibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_UH_Visibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }

        public void ExecuteKantenPoints(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (GleisKantenPointsVisibility == Visibility.Visible)
            {
                GleisKantenPointsVisibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                GleisKantenPointsVisibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteHorizontalPoints(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_LA_PointsVisibility == Visibility.Visible)
            {
                Entwurfselement_LA_PointsVisibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_LA_PointsVisibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteVerticalPoints(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_HO_PointsVisibility == Visibility.Visible)
            {
                Entwurfselement_HO_PointsVisibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_HO_PointsVisibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteMeilagePoints(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_KM_PointsVisibility == Visibility.Visible)
            {
                Entwurfselement_KM_PointsVisibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_KM_PointsVisibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        public void ExecuteCantPoints(object parameter)
        {
            CheckBox box = ((CheckBox)parameter);
            if (Entwurfselement_UH_PointsVisibility == Visibility.Visible)
            {
                Entwurfselement_UH_PointsVisibility = Visibility.Collapsed;
                box.IsChecked = false;
            }
            else
            {
                Entwurfselement_UH_PointsVisibility = Visibility.Visible;
                box.IsChecked = true;
            }
        }
        #endregion
    }
}
