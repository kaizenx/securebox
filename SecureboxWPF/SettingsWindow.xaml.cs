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

namespace SecureboxWPF
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool KeepOriginal;
        private bool KeepEncrypted;
        #region main
        public SettingsWindow()
        {
            InitializeComponent();
            Properties.Settings.Default.Reload();
            KeepOriginal = Properties.Settings.Default.KeepOriginalFiles;
            KeepEncrypted = Properties.Settings.Default.KeepEncryptedFiles;
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Apply();
        }
        # endregion

        #region helper
        private void Apply()
        {
            Properties.Settings.Default.KeepOriginalFiles = CheckBoxKeepOriginal.IsChecked.Value;
            Properties.Settings.Default.KeepEncryptedFiles = CheckBoxKeepEncrypted.IsChecked.Value;
            Properties.Settings.Default.Save();
        }
        #endregion

        private void CheckBoxKeepOriginal_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBoxKeepOriginal.IsChecked = KeepOriginal;
        }

        private void CheckBoxKeepEncrypted_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBoxKeepEncrypted.IsChecked = KeepEncrypted;
        }
    }
}
