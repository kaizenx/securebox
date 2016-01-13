using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using Caliburn;
using FileExplorer.WPF.Defines;
using FileExplorer.Models;
using FileExplorer.WPF.ViewModels;
using FileExplorer;
using FileExplorer.WPF.Utils;
using System.Collections.ObjectModel;
using System.Windows;
using System.Configuration;
using System.IO;
using System.Windows.Input;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.WPF.BaseControls;
using System.Threading;
using DropNet;
using DropNet.Models;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.Defines;

namespace SecureboxWPF
{
    /// <summary>
    /// Interaction logic for Bootstrapper.xaml
    /// </summary>
    public partial class Bootstrapper : Window
    {
        #region Data

        IProfile _profile;
        IProfile _profileEx;
        IProfile _profileSkyDrive;
        IProfile _profileGoogleDrive;

        IProfile[] _profiles;

        //private List<string> _viewModes = new List<string>() { "Icon", "SmallIcon", "Grid" };
        //private string _addPath = lookupPath;
        private IEventAggregator _events;
        private IWindowManager _windowManager;
        private IExplorerViewModel _explorer = null;

        private bool _expandRootDirectories = false;
        private bool _enableDrag = true, _enableDrop = true, _enableMultiSelect = true;
        private string _openPath = "";

        private bool _useScriptCommandInitializer = true;

        private ObservableCollection<IEntryModel> _rootModels = new ObservableCollection<IEntryModel>();
        //private string _fileFilter = "Texts (.txt)|*.txt|Pictures (.jpg, .png)|*.jpg,*.png|Songs (.mp3)|*.mp3|All Files (*.*)|*.*";
        private string _fileFilter = "All Files (*.*)|*.*";
        private DropBoxProfile _profileDropBox;
        private IEntryModel _selectedRootModel;
        private bool _showTabsWhenOneTab;
        #endregion

        #region Public Properties
        public static string skyDriveAliasMask = "{0}'s OneDrive";
        public ObservableCollection<IEntryModel> RootModels { get { return _rootModels; } }
        //public string FileFilter { get { return _fileFilter; } set { _fileFilter = value; NotifyOfPropertyChange(() => FileFilter); } }

        #endregion

        public Bootstrapper()
        {
            InitializeComponent();
            _profile = new FileSystemInfoProfile(_events);
            _profileEx = new FileSystemInfoExProfile(_events, _windowManager, new FileExplorer.Models.SevenZipSharp.SzsProfile(_events));

            Func<string> loginSkyDrive = () =>
            {
                var login = new SkyDriveLogin(AuthorizationKeys.SkyDrive_Client_Id);
                if (_windowManager.ShowDialog(new LoginViewModel(login)).Value)
                {
                    return login.AuthCode;
                }
                return null;
            };

            if (AuthorizationKeys.SkyDrive_Client_Secret != null)
                _profileSkyDrive = new SkyDriveProfile(_events, AuthorizationKeys.SkyDrive_Client_Id, loginSkyDrive, skyDriveAliasMask);


            Func<UserLogin> loginDropBox = () =>
            {
                var login = new DropBoxLogin(AuthorizationKeys.DropBox_Client_Id,
                    AuthorizationKeys.DropBox_Client_Secret);
                if (_windowManager.ShowDialog(new LoginViewModel(login)).Value)
                {
                    return login.AccessToken;
                }
                return null;
            };

            if (AuthorizationKeys.DropBox_Client_Secret != null)
                _profileDropBox = new DropBoxProfile(_events,
                        AuthorizationKeys.DropBox_Client_Id,
                              AuthorizationKeys.DropBox_Client_Secret,
                              loginDropBox);

            if (System.IO.File.Exists("gapi_client_secret.json"))
                using (var gapi_secret_stream = System.IO.File.OpenRead("gapi_client_secret.json")) //For demo only.
                {
                    _profileGoogleDrive = new GoogleDriveProfile(_events, gapi_secret_stream);
                }


            RootModels.Add(AsyncUtils.RunSync(() => _profileEx.ParseAsync(System.IO.DirectoryInfoEx.DesktopDirectory.FullName)));


            _profiles = new IProfile[] {
                _profileEx, _profileSkyDrive, _profileDropBox, _profileGoogleDrive }.Where(p => p != null).ToArray();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            new SecureboxWPF.SecureboxFileExplorer(_profiles, RootModels.ToArray(), _fileFilter, myDocumentsPath).Show();
            this.Close();
        }
    }
}
