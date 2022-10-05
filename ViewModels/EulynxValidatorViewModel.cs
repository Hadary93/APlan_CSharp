using APLan.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace APLan.ViewModels
{
    public class EulynxValidatorViewModel : INotifyPropertyChanged
    {
        #region Inotify essentials
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        
        #region attributes
        private FolderBrowserDialog folderBrowserDialog1;
        private OpenFileDialog openFileDialog1;

        private string xml;
        private string path;
        private string report;
        public string XML
        {
            get { return xml; }
            set
            {
                xml = value;
                OnPropertyChanged();
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }
        public string Report
        {
            get { return report; }
            set
            {
                report = value;
                OnPropertyChanged();
            }
        }
        #endregion
        
        #region commands
        public ICommand FilePath { get; set; }
        public ICommand OutputPath { get; set; }
        public ICommand Validate { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Ok { get; set; }
        public ICommand ValidateXML { get; set; }
        #endregion

        #region constructor
        public EulynxValidatorViewModel()
        {
            
            FilePath = new RelayCommand(ExecuteFilePath);
            OutputPath = new RelayCommand(ExecuteOutputPath);
            Validate = new RelayCommand(ExecuteValidate);
            Cancel = new RelayCommand(ExecuteCancel);
            folderBrowserDialog1 = new FolderBrowserDialog();
            openFileDialog1 = new OpenFileDialog();
        }
        #endregion

        #region logic
        public void ExecuteFilePath(object parameter)
        { 
            openFileDialog1.Filter = "Types (*.xml;*.euxml)|*.xml;*.euxml";
            openFileDialog1.ShowDialog();
            XML = openFileDialog1.FileName;
        }
        public void ExecuteOutputPath(object parameter)
        {
            folderBrowserDialog1.ShowDialog();
            Path = folderBrowserDialog1.SelectedPath;
        }
        public void ExecuteValidate(object parameter)
        {  
            string command = $"/k cd validate & eulynx-validator.exe -s{xml} -o{path} & exit";
            var myProcess=Process.Start("cmd.exe", command);

            Thread.Sleep(2000);

            Report = File.ReadAllText(path+ "/XML Schema Report.txt");
        }
        public void ExecuteCancel(object parameter)
        {
            ((Window)parameter).Close();
        }

        #endregion
    }
}
