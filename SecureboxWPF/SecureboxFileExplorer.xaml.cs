using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Caliburn.Micro;
using FileExplorer.WPF.Defines;
using FileExplorer.Models;
using FileExplorer.WPF.ViewModels;
using FileExplorer;
using FileExplorer.WPF.Utils;
using FileExplorer.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.WPF.BaseControls;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.Defines;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Security.Permissions;

namespace SecureboxWPF
{
    
    /// <summary>
    /// Interaction logic for SecureboxFileExplorer.xaml
    /// </summary>
    public partial class SecureboxFileExplorer : Window
    {

        private IEntryModel[] _rootDirs;
        private string _filterStr;
        private string _selectedPath;
        private IProfile[] _profiles;
        static string dbname = System.IO.Path.Combine(
                                                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                        "Securebox", "locker.sbox");

        private FileSystemWatcher _watcher = new FileSystemWatcher();

        public SecureboxFileExplorer(IProfile[] profiles, IEntryModel[] rootDirs, string mask, string selectedPath = "c:\\")
        {
            InitializeComponent();
            Securebox.CenterWindowOnScreen(this);
            _profiles = profiles;
            _rootDirs = rootDirs;
            _filterStr = mask;
            _selectedPath = selectedPath;
            
            
        }



        void CurrentDirectory_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
           //System.Windows.MessageBox.Show(explorer.ViewModel.CurrentDirectory.ToString());
            if (explorer.ViewModel.CurrentDirectory != null)
            {
                _watcher.Path = explorer.ViewModel.CurrentDirectory.ToString();
            }
            
        }

        

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void WatchSystem(String folderName)
        {
            
            //_currentFolder = explorer.ViewModel.CurrentDirectory.ToString();
            _watcher.Path = folderName;
            /* Watch for changes in LastAccess and LastWrite times, and
           the renaming of files or directories. */
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnChanged);
            _watcher.Deleted += new FileSystemEventHandler(OnChanged);
            _watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Add event handlers.
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnChanged);
            _watcher.Deleted += new FileSystemEventHandler(OnChanged);
            _watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            _watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers. 
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            //Debug.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            this.RefreshView();
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            //Debug.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            this.RefreshView();
        }

        public override void OnApplyTemplate()
        {
            
            base.OnApplyTemplate();
            //FileExplorer.WPF.UserControls.Explorer exp = explorer as FileExplorer.WPF.UserControls.Explorer;

            explorer.ViewModel.Initializer =
           new ScriptCommandInitializer()
           {
               OnModelCreated = IOInitializeHelpers.Explorer_Initialize_Default,
               OnViewAttached = UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot(),
               RootModels = _rootDirs,
               StartupParameters = new ParameterDic()
                {
                        { "Profiles", _profiles },
                        { "RootDirectories", _rootDirs },
                        { "StartupPath", _selectedPath },
                        { "FilterString", _filterStr },
                        { "ViewMode", "List" },
                        { "ItemSize", 4 },
                        { "EnableDrag", true },
                        { "EnableDrop", true },
                        { "FileListNewWindowCommand", NullScriptCommand.Instance }, //Disable NewWindow Command.
                        { "EnableMultiSelect", true},
                        { "ShowToolbar", false },
                        { "ShowGridHeader", false }
                }
           };
            explorer.ViewModel.FileList.EnableContextMenu = false;
            explorer.ViewModel.DirectoryTree.EnableContextMenu = false;
            explorer.ViewModel.FileList.PropertyChanged += CurrentDirectory_PropertyChanged;
            //this.WatchSystem(explorer.ViewModel.CurrentDirectory.ToString());
        }

        private void RefreshView()
        {
            IScriptCommand cmd = UIScriptCommands.FileListRefresh(true);
            explorer.ViewModel.Commands.ExecuteAsync(cmd);
        }

        private void Encrypt_Button_Click(object sender, RoutedEventArgs e)
        {
           
            var _fileList = explorer.ViewModel.FileList.Selection.SelectedItems;
            List<string> encryptedFileNames = new List<string>();
            Boolean isCancelEncryptionProcess = false;
            foreach (var el in _fileList)
            {
                if (System.IO.Path.GetExtension(el.EntryModel.FullPath) == Securebox.secureboxExtension)
                {
                    //System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("File is already encrypted, press \"Cancel\" to skip remaining selected file(s), press \"Ok\" to continue", "File is already encrypted by Securebox", MessageBoxButtons.OKCancel);                    
                    //isCancelEncryptionProcess = dialogResult == System.Windows.Forms.DialogResult.Cancel;       
                }
                else
                {
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();
                    Securebox.EncryptFile(el.EntryModel.FullPath, el.EntryModel.FullPath + ".tmp", g);
                    isCancelEncryptionProcess = Securebox.PostProcessEncryptFile(el.EntryModel.FullPath + ".tmp", el.EntryModel.FullPath + ".box", g);
                    encryptedFileNames.Add(System.IO.Path.GetFileName(el.EntryModel.FullPath));
                }

                if (isCancelEncryptionProcess)
                {
                    break;
                }
            }
            var report = String.Join("\n", encryptedFileNames.ToArray());
            IScriptCommand cmd = UIScriptCommands.FileListRefresh(true, UIScriptCommands.MessageBoxOK("Report", report));
            explorer.ViewModel.Commands.ExecuteAsync(cmd);



            
        }

        private void Decrypt_Button_Click(object sender, RoutedEventArgs e)
        {

            //FileExplorer.WPF.UserControls.Explorer exp = explorer as FileExplorer.WPF.UserControls.Explorer;
            var _fileList = explorer.ViewModel.FileList.Selection.SelectedItems;
            foreach (var el in _fileList)
            {
                //Debug.WriteLine(el.EntryModel.FullPath);
                Securebox.DecryptPreprocess(el.EntryModel.FullPath);
                //Helper.DecryptFile(el.EntryModel.FullPath, el.EntryModel.FullPath + ".sbox", g.ToString());
                // Debug.WriteLine(el.ToString());
            }
            IScriptCommand cmd = UIScriptCommands.FileListRefresh(true, UIScriptCommands.MessageBoxOK("Refresh", "File(s) Decrypted"));
            explorer.ViewModel.Commands.ExecuteAsync(cmd);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            //import db
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = ""; // Default file name 
           // dlg.DefaultExt = ".txt"; // Default file extension 
            dlg.Title = "Import and append Securebox key file";
            dlg.Filter = "Securebox key file (.sbox)|*.sbox"; // Filter files by extension 

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                DatabaseManagement.AppendKeyFile(filename);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            //export db
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog1.Filter = "Securebox key file (.sbox)|*.sbox"; // Filter files by extension 
            saveFileDialog1.Title = "Export and save Securebox key file";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                //System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                File.Copy(DatabaseManagement.GetDBFullPath(), saveFileDialog1.FileName, true);
                IScriptCommand cmd = UIScriptCommands.FileListRefresh(true, UIScriptCommands.MessageBoxOK("Refresh", "Securebox key file exported"));
                explorer.ViewModel.Commands.ExecuteAsync(cmd);
                //fs.Close();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow el = new SettingsWindow();
            el.Owner = this;
            el.ShowDialog();

        }

        private void explorer_Loaded(object sender, RoutedEventArgs e)
        {
            this.WatchSystem(_selectedPath);
            Securebox.CheckIfNeedUpdate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Securebox.CheckIfNeedUpdate();
        }

        
       
       
       
    }

}
