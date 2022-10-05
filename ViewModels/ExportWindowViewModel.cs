using APLan.Commands;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;

namespace APLan.ViewModels
{
    public class ExportWindowViewModel : INotifyPropertyChanged
    {
        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region attributes
        private FolderBrowserDialog folderBrowserDialog1;
        private string outputFolder;
        private string successfull;
        private string euXMLResult;
        private string validateReport;
        public string OutputFolder
        {
            get => outputFolder;
            set
            {
                outputFolder = value;
                OnPropertyChanged();
            }
        }
        public string Successfull
        {
            get { return successfull; }
            set
            {
                successfull = value;
                OnPropertyChanged();
            }
        }
        public string EuXMLResult
        {
            get { return euXMLResult; }
            set
            {
                euXMLResult = value;
                OnPropertyChanged();
            }
        }
        public string ValidateReport
        {
            get { return validateReport; }
            set
            {
                validateReport = value;
                OnPropertyChanged();
            }
        }
        #endregion
        
        #region commands
        public ICommand Cancel { get; set; }
        public ICommand Export { get; set; }
        public ICommand SelectFolder { get; set; }
        public ICommand Ok { get; set; }
        public ICommand ValidateXML { get; set; }
        #endregion

        #region constructor
        public ExportWindowViewModel()
        {
            Cancel = new RelayCommand(ExecuteCancelButton);
            Export = new RelayCommand(ExecuteExportButton);
            SelectFolder = new RelayCommand(ExecuteSelectFolderButton);
            Ok = new RelayCommand(ExecuteOk);
            ValidateXML = new RelayCommand(ExecuteValidateXML);
            folderBrowserDialog1 = new FolderBrowserDialog();
        }
        #endregion

        #region logic
        public void ExecuteCancelButton(object parameter)
        {
            ((Window)parameter).Close();
        }
        public void ExecuteExportButton(object parameter)
        {
            var objects = (object[])parameter;
            var exportType = ((TextBlock)objects[0]).Text;
            var exportPath = ((System.Windows.Controls.TextBox)objects[1]).Text;
            var projectName = ((ComboBoxItem)((System.Windows.Controls.ComboBox)objects[2]).SelectedValue)?.Content;

            if (exportType!=null && projectName!=null && Directory.Exists(exportPath))
            {
                ModelViewModel.eulynxService.serialization(ModelViewModel.eulynx, projectName.ToString(), exportPath);
                Successfull = "Exporting to XML was Successfull";
                XmlReader xmlReader = XmlReader.Create(OutputFolder+ "/eulynx" + projectName.ToString()+".euxml"); 
                Views.ExportConfirmationAndValidation exportConfirmValidate = new();
                exportConfirmValidate.Show();               
            }
            else
            {
                System.Windows.MessageBox.Show("Please check provided information");
            }
        }
        public void ExecuteSelectFolderButton(object parameter)
        {
            folderBrowserDialog1.ShowDialog();
            OutputFolder= folderBrowserDialog1.SelectedPath;
        }
        public void ExecuteOk(object parameter)
        {
            ((Window)parameter).Close();
        }
        public void ExecuteValidateXML(object parameter)
        {
            var projectName = (string)parameter;
            
            string command = $"/k cd validate & eulynx-validator.exe -s{OutputFolder + "/eulynx" + projectName.ToString() + ".euxml"} -o{OutputFolder} & exit";
            var myProcess = Process.Start("cmd.exe", command);

            Thread.Sleep(5000);

            ValidateReport = File.ReadAllText(OutputFolder + "/XML Schema Report.txt");
        }
        #endregion
    }
}
