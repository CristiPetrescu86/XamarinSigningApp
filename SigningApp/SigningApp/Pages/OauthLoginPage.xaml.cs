using LicentaApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp;

namespace SigningApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OauthLoginPage : ContentPage
    {
        public OauthLoginPage()
        {
            InitializeComponent();
        }


        void webviewNavigating(object sender, WebNavigatingEventArgs e)
        {   
            labelLoading.IsVisible = true;

            Match m = Regex.Match(e.Url, @"(?i:http://)(?<localhost>[^\s/]*)/\?(?<code>[^\s/]*)\&(?<state>[^\s]*)");
            if (m.Success)
            {
                string servername = m.Groups["localhost"].Value;
                string username = m.Groups["code"].Value;
                string state = m.Groups["state"].Value;

                if (servername == "localhost:8080")
                {
                    string cod = username.Substring(5);

                    LoginPage.user = new User();
                    LoginPage.user.serviceLink = "https://msign-test.transsped.ro/csc/v0/";
                    LoginPage.user.authModeSelected = "oauth";

                    bool ok = LoginPage.user.oauth2Token(cod, "authorization_code");

                    //LoginPage.user.serviceLink = "https://msign-test.transsped.ro/csc/v1/";

                    if (ok)
                    {
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                    }
                    else
                    {
                        Application.Current.MainPage = new NavigationPage(new LoginPage());
                    }

                }
            }
        }

        void webviewNavigated(object sender, WebNavigatedEventArgs e)
        {
            labelLoading.IsVisible = false;
        }
    }
}