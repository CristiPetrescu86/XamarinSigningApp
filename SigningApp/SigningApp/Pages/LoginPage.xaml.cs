using LicentaApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp.ViewModel;

namespace XamarinLicentaApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public static User user;
        public LoginPage()
        {
            var vm = new LoginViewModel();
            this.BindingContext = vm;
            vm.DisplayInvalidLoginPrompt += () => DisplayAlert("Eroare", "Credentiale gresite, incercati din nou", "OK");
            vm.DisplayNotSetUsername += () => DisplayAlert("Eroare", "Lipseste nume utilizator", "OK");
            vm.DisplayNotSetPassword += () => DisplayAlert("Eroare", "Lipseste parola utilizator", "OK");
            InitializeComponent();

        }


    }
}