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
using Xceed.Wpf.Toolkit;
using IWshRuntimeLibrary;
using Shell32;
using Microsoft.Win32;
using System.IO;
using RestSharp;
using System.Diagnostics;
using System.Windows.Navigation;

namespace SecureboxWPF
{
    
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
       
        string placeHolderpassword = "GOBLIGOCK";
        #region main
        public LoginWindow()
        {
            InitializeComponent();
            Securebox.CenterWindowOnScreen(this);
            DatabaseManagement.CreateDatabase();
        }
        #endregion
        #region helper methods
        private bool ValidateForm(string username, string password)
        {
            ValidationMessage.Content = "";
            if (username == "")
            {
                ValidationMessage.Content = "Username empty";
            }
            else if (password == "" || password == placeHolderpassword)
            {
                ValidationMessage.Content = "Password empty";
            }
            return ValidationMessage.Content == null || ValidationMessage.Content.ToString().Length == 0;
        }
        

        private void LoadMainExplorerWindow()
        {

        }
        private void SignIn(String username, String password)
        {
            bool isValid = this.ValidateForm(username, password);
            if (isValid)
            {
                SignInButton.IsEnabled = false;
                var client = new RestClient("https://securebox.io/");
                var request = new RestRequest("api-token-auth/", Method.POST);
                request.AddParameter("username", username); // adds to POST or URL querystring based on Method
                request.AddParameter("password", password); // adds to POST or URL querystring based on Method
                IRestResponse<MyToken> response = client.Execute<MyToken>(request);

                try
                {
                    var token = response.Data.Token;
                    if (token != null)
                    {
                        this.RedirectToMainForm();
                    }
                    else
                    {
                        ValidationMessage.Content = "Invalid login";
                        SignInButton.IsEnabled = true;
                    }
                    //MessageBox.Show(token.ToString());
                    //System.Windows.MessageBox.Show(token.ToString());
                }
                catch
                {
                    var content = response;
                    System.Windows.MessageBox.Show("Error");
                }
            }
        }
        private void RedirectToMainForm()
        {
           
            Bootstrapper e = new Bootstrapper();
            e.Show();
            this.Close();
            //SecureboxFileExplorer secureboxFileExplorer = new SecureboxFileExplorer();
            //secureboxFileExplorer.Show();
            //this.Close();
        }

        private void RedirectToRegistration()
        {

            RegistrationWindow registrationForm = new RegistrationWindow();
            registrationForm.Show();
            this.Close();
        }
        #endregion

        #region event handlers
        private void passwordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password == placeHolderpassword)
            {
                passwordBox.Password = "";
            }
        }

        private void passwordLostFocus(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password.Length <= 0)
            {
                passwordBox.Password = placeHolderpassword;
            }
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string username = userName.Text;
                string password = passwordBox.Password;
                this.SignIn(userName.Text, passwordBox.Password);
                
            }
        }
        private void loaded(object sender, RoutedEventArgs e)
        {
            passwordBox.Password = placeHolderpassword;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //Boolean isChecked = this.checkBox1.Checked;
            //this.RegisterInStartup(isChecked);
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = userName.Text;
            string password = passwordBox.Password;
            this.SignIn(userName.Text, passwordBox.Password);
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            this.RedirectToRegistration();
        }
        #endregion

        

      
        

        
        
       

        
    }
}
