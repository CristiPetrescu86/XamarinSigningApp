using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp.ViewModel;

namespace XamarinLicentaApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KeysInfo : ContentPage
    {
        public KeysInfo()
        {
            LoginPage.user = new LicentaApp.User("adobedemo", "password");
            LoginPage.user.authLogin();
            LoginPage.user.credentialsList();

            var vm = new KeysInfoViewModel();
            this.BindingContext = vm;

            //Debug.WriteLine(LoginPage.user.credentialsIDs[0]);
            foreach (var keyName in LoginPage.user.credentialsIDs)
            {
                vm.KeyName.Add(keyName);
            }


            //foreach (var keyName in LoginPage.user.credentialsIDs)
            //{
            //    LoginPage.user.credentialsInfo(keyName);
            //}
            //Debug.WriteLine(LoginPage.user.keysInfo[0].credentialName);
            //Debug.WriteLine(LoginPage.user.keysInfo[0].authMode);
            LoginPage.user.authRevoke();

            InitializeComponent();
        }
    }
}