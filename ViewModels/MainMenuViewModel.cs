using APLan.Commands;
using APLan.Views;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using APLan.HelperClasses;
using System.Windows.Media;
using System.Windows.Forms;
using aplan.eulynx;

namespace APLan.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
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
        public string SavePath
        {
            get;
            set;
        }
        #endregion

        #region commands
        public ICommand NewProject { get; set; }
        public ICommand AddData { get; set; }
        public ICommand PreviewData { get; set;}
        public ICommand RemoveData { get; set; }
        public ICommand Print { get; set; }
        public ICommand EulynxValidator { get; set; }
        public ICommand ExitProgram { get; set; }
        public ICommand AboutWPF { get; set; }
        public ICommand Save { get; set; }

        #endregion
        
        #region Constructor
        public MainMenuViewModel()
        {
            Save = new RelayCommand(ExecuteSave);
            NewProject = new RelayCommand(ExecuteNewProjectWindow);
            AddData = new RelayCommand(ExecuteAddDataWindow);
            PreviewData = new RelayCommand(ExecutePreviewData);
            RemoveData = new RelayCommand(ExecuteRemoveData);
            Print = new RelayCommand(ExecutePrint);
            EulynxValidator = new RelayCommand(ExecuteEulynxValidator);
            AboutWPF = new RelayCommand(ExecuteAboutWPF);
            ExitProgram = new RelayCommand(ExecuteExitProgram);
            folderBrowserDialog1 = new();
        }
        #endregion
        
        #region logic
        public void ExecuteNewProjectWindow (object parameter)
        {
            
            NewProjectWindow newProject = new();
            newProject.ShowDialog();
        }
        public void ExecuteAddDataWindow(object parameter)
        {
            AddDataWindow addData = new();
            addData.ShowDialog();
        }
        public void ExecutePreviewData(object parameter)
        {
            PreviewDataWindow previewData = new ();
            previewData.ShowDialog();
        }
        public void ExecuteRemoveData(object parameter)
        {
            RemoveDataWindow removeData = new ();
            removeData.ShowDialog();
        }
        public void ExecutePrint(object parameter)
        {
            System.Windows.Controls.PrintDialog printDlg = new ();
            printDlg.ShowDialog();
        }
        public void ExecuteEulynxValidator(object parameter)
        {
            EulynxValidator validator = new ();
            validator.ShowDialog();
        }
        public void ExecuteAboutWPF(object parameter)
        {
            AboutWPF wpfinfo = new ();
            wpfinfo.ShowDialog();
        }
        public void ExecuteExitProgram(object parameter)
        {
            ((MainWindow)parameter).Close();
        }
        public void ExecuteSave(object parameter)
        {
            System.Windows.MessageBox.Show(DrawViewModel.toBeStored.Count.ToString());
            folderBrowserDialog1.ShowDialog();
            SavePath = folderBrowserDialog1.SelectedPath;

            if (Directory.Exists(SavePath))
            {
                List<CanvasObjectInformation> allInfo = new();
                foreach (UIElement element in DrawViewModel.toBeStored)
                {
                    Matrix transformation = element.RenderTransform.Value;
                    transformation.Translate(
                            Canvas.GetLeft(element), // initial from dragging when it comes to the canvas
                            Canvas.GetTop(element) // initial from dragging when it comes to the canvas

                        );
                    if (element is CustomCanvasSignal)
                    {
                        CanvasObjectInformation info = new()
                        {
                            Type = element.GetType().ToString(),
                            //Rotation = CustomProperites.GetRotation(element),
                            SignalImageSource = (((Image)element).Source).ToString(),
                            //LocationInCanvas = CustomProperites.GetLocation(element),
                            RenderTransformMatrix = transformation,
                            Scale = DrawViewModel.signalSizeForConverter
                        };
                        allInfo.Add(info);
                    }
                    else if (element is CustomCanvasText)
                    {
                        CanvasObjectInformation info = new()
                        {
                            Type = element.GetType().ToString(),
                            RenderTransformMatrix = transformation,
                            Scale = DrawViewModel.signalSizeForConverter,
                            IncludedText = ((CustomCanvasText)element).Text,
                            IncludedTextSize = ((CustomCanvasText)element).FontSize

                        };
                        allInfo.Add(info);
                    }
                }
                if (ModelViewModel.eulynx != null)
                {
                    var eulynxService = EulynxService.getInstance();
                    eulynxService.serialization(ModelViewModel.eulynx, "", SavePath);      
                }


                //serialize
                BinaryFormatter bf = new();

                FileStream fsout = new(SavePath + "/save.APlan", FileMode.Create, FileAccess.Write, FileShare.None);

                using (fsout)
                {
                    bf.Serialize(fsout, allInfo);
                }
                fsout.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Saving Directory don't Exist");
            }
        }
            
        #endregion
    }
}
