using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using APLan.Commands;
using System.Collections.ObjectModel;
using APLan.HelperClasses;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using aplan.eulynx;

namespace APLan.ViewModels
{
    public class NewProjectViewModel : INotifyPropertyChanged
    {
        #region Inotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region attributes

        private FolderBrowserDialog folderBrowserDialog1;
        private OpenFileDialog openFileDialog1;

        private bool saveButtonActive = true;  //remove true.
        private bool saveAsButtonActive;
        private bool printButtonActive;
        private bool importButtonActive;
        private string welcomeInfo;
        private string country;
        private string format;
        private string jsonFiles = null;
        private string entwurfselement_KM = null;
        private string gleiskanten = null;
        private string gleisknoten = null;
        private string entwurfselement_LA = null;
        private string entwurfselement_HO = null;
        private string entwurfselement_UH = null;
        private string mdb = null;
        private string euxml = null;
        private string ppxml = null;
        private string projectPath = null;
        private static string currentProjectPath = null;
        private string projectName = null;

        private string OpenProjectPath { get; set; }
        public string ProjectName
        {
            get { return projectName; }
            set
            {
                projectName = value;
                currentProjectPath = projectName;
                OnPropertyChanged();
            }
        }
        public string ProjectPath
        {
            get { return projectPath; }
            set
            {
                projectPath = value;
                OnPropertyChanged();
            }
        }
        public string Country
        {
            get { return country; }
            set
            {
                country = value.Split(':')[1].Trim();
                OnPropertyChanged();
            }
        }
        public string Format
        {
            get { return format; }
            set
            {
                format = value.Split(':')[1].Trim();
                OnPropertyChanged();
            }
        }
        public string JsonFiles
        {
            get { return jsonFiles; }
            set
            {
                jsonFiles = value;
                OnPropertyChanged();
            }
        }
        public string Entwurfselement_KM
        {
            get { return entwurfselement_KM; }
            set
            {
                entwurfselement_KM = value;
                OnPropertyChanged();
            }
        }
        public string Gleiskanten
        {
            get { return gleiskanten; }
            set
            {
                gleiskanten = value;
                OnPropertyChanged();
            }
        }
        public string Gleisknoten
        {
            get { return gleisknoten; }
            set
            {
                gleisknoten = value;
                OnPropertyChanged();
            }
        }
        public string Entwurfselement_LA
        {
            get { return entwurfselement_LA; }
            set
            {
                entwurfselement_LA = value;
                OnPropertyChanged();
            }
        }
        public string Entwurfselement_HO
        {
            get { return entwurfselement_HO; }
            set
            {
                entwurfselement_HO = value;
                OnPropertyChanged();
            }
        }
        public string Entwurfselement_UH
        {
            get { return entwurfselement_UH; }
            set
            {
                entwurfselement_UH = value;
                OnPropertyChanged();
            }
        }
        public string MDB
        {
            get { return mdb; }
            set
            {
                mdb = value;
                OnPropertyChanged();
            }
        }
        public string EUXML
        {
            get { return euxml; }
            set
            {
                euxml = value;
                OnPropertyChanged();
            }
        }
        public string PPXML
        {
            get { return ppxml; }
            set
            {
                ppxml = value;
                OnPropertyChanged();
            }
        }
        public bool SaveButtonActive
        {
            get { return saveButtonActive; }
            set
            {
                saveButtonActive = value;
                OnPropertyChanged();
            }
        }
        public bool SaveAsButtonActive
        {
            get { return saveAsButtonActive; }
            set
            {
                saveAsButtonActive = value;
                OnPropertyChanged();
            }
        }
        public bool PrintButtonActive
        {
            get { return printButtonActive; }
            set
            {
                printButtonActive = value;
                OnPropertyChanged();
            }
        }
        public bool ImportButtonActive
        {
            get { return importButtonActive; }
            set
            {
                importButtonActive = value;
                OnPropertyChanged();
            }
        }
        public string WelcomeInfo
        {
            get { return welcomeInfo; }
            set
            {
                welcomeInfo = value;
                OnPropertyChanged();
            }
        }
        
        private Visibility welcomeVisibility;
        private Visibility _gleisKantenPointsVisibility;
        public Visibility GleisKantenPointsVisibility
        {
            get => _gleisKantenPointsVisibility;
            set
            {
                _gleisKantenPointsVisibility = value;
                OnPropertyChanged();
            }

        }
        public Visibility WelcomeVisibility
        {
            get { return welcomeVisibility; }
            set
            {
                welcomeVisibility = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Point> KantenPointsHoaX
        {
            get;
            set;
        }
        public ObservableCollection<CanvasObjectInformation> loadedObjects
        {
            get;
            set;
        }
        public ObservableCollection<CustomPolyLine> gleiskantenList
        {
            get;
            set;
        }
        public ObservableCollection<Point> gleiskantenPointsList
        {
            get;
            set;
        }
        public ObservableCollection<CustomPolyLine> Entwurfselement_LA_list
        {
            get;
            set;
        }
        public ObservableCollection<Point> Entwurfselement_LAPointsList
        {
            get;
            set;
        }
        public ObservableCollection<CustomPolyLine> Entwurfselement_KM_list
        {
            get;
            set;
        }
        public ObservableCollection<Point> Entwurfselement_KMPointsList
        {
            get;
            set;
        }
        public ObservableCollection<CustomPolyLine> Entwurfselement_HO_list
        {
            get;
            set;
        }
        public ObservableCollection<Point> Entwurfselement_HOPointsList
        {
            get;
            set;
        }
        public ObservableCollection<CustomPolyLine> Entwurfselement_UH_list
        {
            get;
            set;
        }
        public ObservableCollection<Point> Entwurfselement_UHPointsList
        {
            get;
            set;
        }
        public ObservableCollection<CustomNode> gleisknotenList
        {
            get;
            set;
        }

        #endregion

        #region commands
        public ICommand KantenPoints { get; set; }
        public ICommand AddPath { get; set; }
        public ICommand BrowseJson { get; set; }
        public ICommand BrowseMDB { get; set; }
        public ICommand BrowseEuxml { get; set; }
        public ICommand BrowsePpxml { get; set; }
        public ICommand Create { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Open { get; set; }
        #endregion

        #region constructor
        public NewProjectViewModel()
        {
            AddPath = new RelayCommand(ExecuteAddPath);
            BrowseJson = new RelayCommand(ExecuteBrowseJson);
            BrowseMDB = new RelayCommand(ExecuteBrowseMDB);
            BrowseEuxml = new RelayCommand(ExecuteBrowseEuxml);
            BrowsePpxml = new RelayCommand(ExecuteBrowsePpxml);
            Create = new RelayCommand(ExecuteCreate);
            Cancel = new RelayCommand(ExecuteCancel);
            Open = new RelayCommand(openButton);
            folderBrowserDialog1 = new FolderBrowserDialog();
            openFileDialog1 = new OpenFileDialog();

            loadedObjects = new ObservableCollection<CanvasObjectInformation>(); //binded to view

            gleiskantenList = new ObservableCollection<CustomPolyLine>();
            gleiskantenPointsList = new ObservableCollection<Point>();

            Entwurfselement_LA_list = new ObservableCollection<CustomPolyLine>();
            Entwurfselement_KM_list = new ObservableCollection<CustomPolyLine>();
            Entwurfselement_HO_list = new ObservableCollection<CustomPolyLine>();
            Entwurfselement_UH_list = new ObservableCollection<CustomPolyLine>();

            gleisknotenList = new ObservableCollection<CustomNode>();
            Entwurfselement_LAPointsList = new ObservableCollection<Point>();
            Entwurfselement_KMPointsList = new ObservableCollection<Point>();
            Entwurfselement_HOPointsList = new ObservableCollection<Point>();
            Entwurfselement_UHPointsList = new ObservableCollection<Point>();

            KantenPointsHoaX = new ObservableCollection<Point>();

            KantenPoints = new RelayCommand(ExecuteKantenPoints);

            WelcomeInfo = "Welcome";
        }
        #endregion

        #region logic
        public void createModel(string format)
        {

                WelcomeInfo = "Creating Eulynx Object...";
 

                if (format.Equals(".json"))
                {
                    

                    APLan.ViewModels.DrawViewModel.model = new ModelViewModel(
                     country,
                     format,
                     entwurfselement_KM,
                     gleiskanten,
                     gleisknoten,
                     entwurfselement_LA,
                     entwurfselement_HO,
                     entwurfselement_UH,
                     mdb
                     );
           
            

                DrawViewModel.model.drawObject(ViewModels.DrawViewModel.sharedCanvasSize,
                gleiskantenList,
                gleiskantenPointsList,
                Entwurfselement_LA_list,
                Entwurfselement_LAPointsList,
                Entwurfselement_KM_list,
                Entwurfselement_KMPointsList,
                Entwurfselement_HO_list,
                Entwurfselement_HOPointsList,
                Entwurfselement_UH_list,
                Entwurfselement_UHPointsList,
                gleisknotenList);
            }else if (format.Equals(".mdb"))
            {
                WelcomeInfo = "Creating Eulynx Object...";
                APLan.ViewModels.DrawViewModel.model = new ModelViewModel(
                country,
                format,
                null,
                null,
                null,
                null,
                null,
                null,
                mdb
                );
                
                WelcomeInfo = "Drawing...";
                DrawViewModel.model.drawObject(ViewModels.DrawViewModel.sharedCanvasSize,
               gleiskantenList,
               gleiskantenPointsList,
               Entwurfselement_LA_list,
               Entwurfselement_LAPointsList,
               Entwurfselement_KM_list,
               Entwurfselement_KMPointsList,
               Entwurfselement_HO_list,
               Entwurfselement_HOPointsList,
               Entwurfselement_UH_list,
               Entwurfselement_UHPointsList,
               gleisknotenList);
            }
            WelcomeVisibility = Visibility.Collapsed;
           
        }
        public void ExecuteAddPath(object parameter)
        {
            folderBrowserDialog1.ShowDialog();
            ProjectPath = folderBrowserDialog1.SelectedPath;

        }
        public void ExecuteBrowseJson(object parameter)
        {
            clearOldSelectedFiles();
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "Types (*.geojson;*.json)|*.json;*.geojson";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileNames.Length<6 && openFileDialog1.FileNames.Length>=1)
            {
                System.Windows.MessageBox.Show("project Creation needs all the files");
            }
            else if(openFileDialog1.FileNames.Length > 1)
            {
                JsonFiles = "MultiSelectedfiles";
                attachFilesTotheirNames(openFileDialog1.FileNames);
            }
            checkJsonFilesCompleteness(parameter);

        }
        public void ExecuteBrowseMDB(object parameter)
        {
            clearOldSelectedFiles();
            openFileDialog1.Filter = "Types (*.MDB)|*.MDB";
            openFileDialog1.ShowDialog();
            MDB = openFileDialog1.FileName;
            if (MDB!=null)
            {
                ((System.Windows.Controls.Button)parameter).IsEnabled = true;
            }
        }
        public void ExecuteBrowseEuxml(object parameter)
        {
            clearOldSelectedFiles();
            openFileDialog1.Filter = "Types (*.euxml)|*.euxml";
            openFileDialog1.ShowDialog();
            EUXML = openFileDialog1.FileName;
            if (EUXML != null)
            {
                ((System.Windows.Controls.Button)parameter).IsEnabled = true;
            }
        }
        public void ExecuteBrowsePpxml(object parameter)
        {
            clearOldSelectedFiles();
            openFileDialog1.Filter = "Types (*.ppxml)|*.ppxml";
            openFileDialog1.ShowDialog();
            PPXML = openFileDialog1.FileName;
            if (PPXML != null)
            {
                ((System.Windows.Controls.Button)parameter).IsEnabled = true;
            }

        }
        public void ExecuteCreate(object parameter)
        {
            var parameters = (object[])parameter;
            if (checkProjectNameAndPath()==true)
            {
                if (creatProjectFolder() == true)
                {
                    ((Window)parameters[1]).Close();
                    //WelcomeVisibility = Visibility.Collapsed;
                    activateButtons();
                    createModel(parameters[0].ToString());
                }
            }
            
        }
        public void ExecuteCancel(object parameter)
        {
            (parameter as Window)?.Close();
        }
        public void attachFilesTotheirNames(string[] files)
        {
            foreach (string file in files)
            {
                string name = file.Split('.')[0].Split('\\')[file.Split('.')[0].Split('\\').Length-1];
                switch (name)
                {
                    case "Entwurfselement_HO":
                        Entwurfselement_HO = file;
                        break;

                    case "Entwurfselement_KM":
                        Entwurfselement_KM = file;
                        break;
                    case "Entwurfselement_LA":
                        Entwurfselement_LA = file;
                        break;

                    case "Entwurfselement_UH":
                        Entwurfselement_UH = file;
                        break;
                    case "Gleiskanten":
                        Gleiskanten = file;
                        break;

                    case "Gleisknoten":
                        Gleisknoten = file;
                        break;

                    default:
                       
                        break;
                }
            }
        }
        public void checkJsonFilesCompleteness(object parameter)
        {
            if (entwurfselement_KM != null &&
                entwurfselement_LA != null &&
                entwurfselement_HO != null &&
                entwurfselement_UH != null &&
                gleiskanten != null &&
                gleisknoten != null)
            {

                ((System.Windows.Controls.Button)parameter).IsEnabled = true;
            }
        }
        public void clearOldSelectedFiles()
        {
            Entwurfselement_KM = null;
            entwurfselement_LA = null;
            entwurfselement_HO = null;
            entwurfselement_UH = null;
            Gleiskanten = null;
            Gleisknoten = null;
            MDB = null;
            EUXML = null;
            PPXML = null;
        }
        public bool checkProjectNameAndPath()
        {
            bool flag = true;
            if (ProjectName==null)
            {
                flag = false;
                System.Windows.MessageBox.Show("please enter a project name");
            }else if (ProjectPath == null)
            {
                flag = false;
                System.Windows.MessageBox.Show("please select a project path");
            }
            return flag;
        }
        public bool creatProjectFolder()
        {
            bool flag = true;
            string targetFolderPath = projectPath + "/" + $"{projectName}";
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }
            else
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Overrider Project?", "Project Exists", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    return false;
                }
            }
            DirectoryInfo directory = new DirectoryInfo(targetFolderPath);
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            transferFilesToPath(targetFolderPath);
            return flag;
        }
        public void transferFilesToPath(string path)
        {
            if(Entwurfselement_KM != null)
            {
                File.Copy(Entwurfselement_KM, path+"/"+Path.GetFileName(Entwurfselement_KM), true);
            }
            if (entwurfselement_LA != null)
            {
                File.Copy(Entwurfselement_LA, path + "/" + Path.GetFileName(Entwurfselement_LA), true);
            }
            if (entwurfselement_HO != null)
            {
                File.Copy(Entwurfselement_HO, path + "/" + Path.GetFileName(Entwurfselement_HO), true);
            }
            if (Entwurfselement_UH != null)
            {
                File.Copy(Entwurfselement_UH, path + "/" + Path.GetFileName(Entwurfselement_UH), true);
            }
            if (Gleiskanten != null)
            {
                File.Copy(Gleiskanten, path + "/" + Path.GetFileName(Gleiskanten), true);
            }
            if (Gleisknoten != null)
            {
                File.Copy(Gleisknoten, path + "/" + Path.GetFileName(Gleisknoten), true);
            }
            if (MDB != null)
            {
                File.Copy(MDB, path + "/" + Path.GetFileName(MDB), true);
            }
            if (EUXML != null)
            {
                File.Copy(EUXML, path + "/" + Path.GetFileName(EUXML), true);
            }
            if (PPXML != null)
            {
                File.Copy(PPXML, path + "/" + Path.GetFileName(PPXML), true);
            }
        }
        public void activateButtons()
        {
            SaveButtonActive = true;
            SaveAsButtonActive = true;
            //ImportButtonActive = true;
            PrintButtonActive = true;
        }
        public void ExecuteKantenPoints(object parameter)
        {
            System.Windows.Controls.CheckBox box = ((System.Windows.Controls.CheckBox)parameter);
            if (box.IsChecked==true)
            {
                //for (int i = 0; i < gleiskantenPointsList.Count; i++)
                //{
                //    KantenPointsHoaX.Add(gleiskantenPointsList[i]);
                //}
                KantenPointsHoaX = gleiskantenPointsList;
                System.Windows.MessageBox.Show("Hello");
            }
            else
            {
                KantenPointsHoaX.Clear();
            }
        }
        public void openButton(object parameter)
        {
            folderBrowserDialog1.ShowDialog();
            OpenProjectPath = folderBrowserDialog1.SelectedPath;

            if (Directory.Exists(OpenProjectPath))
            {
                List<CanvasObjectInformation> importedObjects = new List<CanvasObjectInformation>();
                BinaryFormatter bfDeserialize = new BinaryFormatter();

                foreach (string f in Directory.GetFiles(OpenProjectPath))
                {
                    if (System.IO.Path.GetExtension(f) == ".APlan")
                    {

                        FileStream fsin = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None);
                        fsin.Position = 0;
                        importedObjects = (List<CanvasObjectInformation>)bfDeserialize.Deserialize(fsin);
                        foreach (CanvasObjectInformation ObjectInfo in importedObjects)
                        {
                            loadedObjects.Add(ObjectInfo);

                        }
                        fsin.Close();
                    }
                    else if (System.IO.Path.GetExtension(f) == ".euxml")
                    {
                        var eulynxService = EulynxService.getInstance();
                        ModelViewModel.eulynx = eulynxService.deserialization(f);
                        gleiskantenList.Clear();
                        gleiskantenPointsList.Clear();
                        Entwurfselement_LA_list.Clear();
                        Entwurfselement_LAPointsList.Clear();
                        Entwurfselement_KM_list.Clear();
                        Entwurfselement_KMPointsList.Clear();
                        Entwurfselement_HO_list.Clear();
                        Entwurfselement_HOPointsList.Clear();
                        Entwurfselement_UH_list.Clear();
                        Entwurfselement_UHPointsList.Clear();
                        gleisknotenList.Clear();

                        ModelViewModel model = new();
                        model.drawObject(ViewModels.DrawViewModel.sharedCanvasSize,
                        gleiskantenList,
                        gleiskantenPointsList,
                        Entwurfselement_LA_list,
                        Entwurfselement_LAPointsList,
                        Entwurfselement_KM_list,
                        Entwurfselement_KMPointsList,
                        Entwurfselement_HO_list,
                        Entwurfselement_HOPointsList,
                        Entwurfselement_UH_list,
                        Entwurfselement_UHPointsList,
                        gleisknotenList); 
                    }
                }
            }
        }
        #endregion
    }
}
