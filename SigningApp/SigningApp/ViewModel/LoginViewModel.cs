using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace XamarinLicentaApp.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public Action DisplayInvalidLoginPrompt;
        public Action DisplayNotSetUsername;
        public Action DisplayNotSetPassword;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Username"));
            }
        }
        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Password"));
            }
        }


        private Command onSubmit;

        public ICommand OnSubmit
        {
            get
            {
                if (onSubmit == null)
                {
                    onSubmit = new Command(PerformOnSubmit);
                }

                return onSubmit;
            }
        }

        private void PerformOnSubmit()
        {
            //if(username == null)
            //{
            //    DisplayNotSetUsername();
            //    return;
            //}

            //if (password == null)
            //{
            //    DisplayNotSetPassword();
            //    return;
            //}
                
            //LoginPage.user = new LicentaApp.User(username,password);
            LoginPage.user = new LicentaApp.User("adobedemo","password");
            
            bool ok = LoginPage.user.authLogin();
            
            if(ok == true)
            {
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                Username = null;
                Password = null;
                DisplayInvalidLoginPrompt();
            }
        }
    }
}
