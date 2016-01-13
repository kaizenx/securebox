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
using RestSharp;

namespace SecureboxWPF
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        string placeHolderpassword = "GOBLIGOCK";

        public RegistrationWindow()
        {
            InitializeComponent();
            Securebox.CenterWindowOnScreen(this);
        }
        private void ToggleRegistrationButton()
        {
            RegisterUser.IsEnabled = !RegisterUser.IsEnabled;
        }

        private void passwordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordTextBox.Password == placeHolderpassword)
            {
                passwordTextBox.Password = "";
            }
        }

        private void passwordLostFocus(object sender, RoutedEventArgs e)
        {
            //if no password was filled, fill with placeholder again
            if (passwordTextBox.Password.Length <= 0)
            {
                passwordTextBox.Password = placeHolderpassword;
            }
            this.ValidateForm(sender, e);
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            passwordTextBox.Password = placeHolderpassword;
        }

        private void passwordConfirmedChanged(object sender, RoutedEventArgs e)
        {
            if (passwordConfirmTextBox.Password == placeHolderpassword)
            {
                passwordConfirmTextBox.Password = "";
            }
        }

        private void passwordConfirmedLostFocus(object sender, RoutedEventArgs e)
        {
            //if no password was filled, fill with placeholder again
            if (passwordConfirmTextBox.Password.Length <= 0)
            {
                passwordConfirmTextBox.Password = placeHolderpassword;
            }
            this.ValidateForm(sender, e);
        }

        private void loadedConfirmed(object sender, RoutedEventArgs e)
        {
            passwordConfirmTextBox.Password = placeHolderpassword;
        }

        private void RedirectToMainForm()
        {
            //SecureboxFileExplorer secureboxFileExplorer = new SecureboxFileExplorer();
            //secureboxFileExplorer.Show();
            //this.Close();
        }

        private void SignUp(String username, String firstName, String lastName, String email, String password, String password2, bool checkboxAgree)
        {
            var client = new RestClient("https://securebox.io/");
            var request = new RestRequest("create_new_user/", Method.POST);

            request.AddParameter("username", username); // adds to POST or URL querystring based on Method
            request.AddParameter("firstname", firstName); // adds to POST or URL querystring based on Method
            request.AddParameter("lastname", lastName); // adds to POST or URL querystring based on Method
            request.AddParameter("email", email); // adds to POST or URL querystring based on Method
            request.AddParameter("password", password); // adds to POST or URL querystring based on Method

            IRestResponse<MyToken> response = client.Execute<MyToken>(request);
            try
            {
                var token = response.Data.Token;
                if (token.Length > 0)
                {
                    this.RedirectToMainForm();
                }
            }
            catch
            {
                var content = response;
                this.ToggleRegistrationButton();
                ValidationMessage.Text = "Registration has failed, please try again";
            }
        }

        private void ValidateForm(object sender, RoutedEventArgs e)
        {
            String userName = userNameTextBox.Text;
            String firstName = firstNameTextBox.Text;
            String lastName = lastNameTextBox.Text;
            String email = emailTextBox.Text;
            String password = passwordTextBox.Password;
            String password2 = passwordConfirmTextBox.Password;
            bool checkboxAgree = CheckboxAgree.IsChecked.Value;

            RegisterUser.IsEnabled = this.ValidateForm(userName, firstName, lastName, email, password, password2, checkboxAgree);

            
        }

        private bool ValidateForm(String userName, String firstName, String lastName, String email, String password, String password2, bool checkboxAgree)
        {
            

            ValidationMessage.Text = "";
            //check if empty
            if (userName == "")
            {
                ValidationMessage.Text = "Username required!";
            }
            else if (firstName == "")
            {
                ValidationMessage.Text = "First name required!";
            }
            else if (lastName == "")
            {
                ValidationMessage.Text = "Last name required!";
            }
            else if (email == "")
            {
                ValidationMessage.Text = "Email required!";
            }
            else if (password == "")
            {
                ValidationMessage.Text = "Password required!";
            }
            else if (password2 == "")
            {
                ValidationMessage.Text = "Password confirmation required!";
            }
            //check if passwords match
            if( ValidationMessage.Text.Length <= 0)
            {
                Securebox util = new Securebox();
                if (password != password2)
                {
                    ValidationMessage.Text = "Passwords do not match!";
                }
                else if (!util.IsValidEmail(emailTextBox.Text))
                {
                    ValidationMessage.Text = "Invalid email!";
                }
            }

            return ValidationMessage.Text.Length == 0;
        }

        

        private void signUpButton_Click(object sender, EventArgs e)
        {
            this.ToggleRegistrationButton();
            String userName = userNameTextBox.Text;
            String firstName = firstNameTextBox.Text;
            String lastName = lastNameTextBox.Text;
            String email = emailTextBox.Text;
            String password = passwordTextBox.Password;
            String password2 = passwordConfirmTextBox.Password;
            bool checkboxAgree = CheckboxAgree.IsChecked.Value;

            bool isValidate = this.ValidateForm(userName, firstName, lastName, email, password, password2, checkboxAgree);
            
            if( isValidate )
            {
                this.SignUp(userName, firstName, lastName, email, password, password2, checkboxAgree);
            }
            else
            {
                this.ToggleRegistrationButton();
            }
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
