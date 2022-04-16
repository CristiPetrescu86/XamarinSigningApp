using SigningApp.Pages;
using SigningApp.PopupPages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
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
            if (username == null)
            {
                DisplayNotSetUsername();
                return;
            }

            if (password == null)
            {
                DisplayNotSetPassword();
                return;
            }

            LoginPage.user = new LicentaApp.User(username, password);
            //LoginPage.user = new LicentaApp.User("adobedemo","password");

            bool ok = LoginPage.user.authLogin(false);

            if (ok == true)
            {
                LoginPage.user.authModeSelected = "explicit";
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                Username = null;
                Password = null;
                DisplayInvalidLoginPrompt();
            }
        }


        private Command changeLogin;

        public ICommand ChangeLogin
        {
            get
            {
                if (changeLogin == null)
                {
                    changeLogin = new Command(ChangeLoginPage);
                }

                return changeLogin;
            }
        }

        private async void ChangeLoginPage()
        {
            var newPage = new OauthLoginPage();
            await Application.Current.MainPage.Navigation.PushAsync(newPage);
        }




        private Command logAux;

        public ICommand LogAux
        {
            get
            {
                if (logAux == null)
                {
                    logAux = new Command(LogFunc);
                }

                return logAux;
            }
        }


        public string Token { get; set; }

        private async void LogFunc()
        {
            LoginPage.user = new LicentaApp.User();
            LoginPage.user.setAccess(Token);
            LoginPage.user.authModeSelected = "oauth";

            var newPage = new MainPage();
            await Application.Current.MainPage.Navigation.PushAsync(newPage);
        }


    }
}
