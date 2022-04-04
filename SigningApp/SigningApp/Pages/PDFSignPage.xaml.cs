using SigningApp.Model;
using SigningApp.PopupPages;
using SigningApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;


namespace XamarinLicentaApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PDFSignPage : ContentPage
    {
        public PDFSignPage()
        {
            InitializeComponent();

            var vm = new PDFSignPageViewModel();
            this.BindingContext = vm;
            vm.DisplayKeyNotSelected += () => DisplayAlert("Eroare", "Key Not Selected", "Close");
            vm.DisplayXCoordNotSet += () => DisplayAlert("Eroare", "X nu are valoare", "Close");
            vm.DisplayYCoordNotSet += () => DisplayAlert("Eroare", "Y nu are valoare", "Close");
            vm.DisplayWidthNotSet += () => DisplayAlert("Eroare", "Latimea chenarului nu are valoare", "Close");
            vm.DisplayHeightNotSet += () => DisplayAlert("Eroare", "Lungimea chenarului nu are valoare", "Close");
            vm.DisplayFileNotUploaded += () => DisplayAlert("Eroare", "Fisierul nu a fost incarcat", "Close");
            vm.DisplayVisibilityNotSet += () => DisplayAlert("Eroare", "Vizibilitate chenar nu a fost setata", "Close");
            vm.DisplayPageNotSelected += () => DisplayAlert("Eroare", "Amplasamentul pe pagina nu a fost setat", "Close");
            vm.DisplayCoordError += () => DisplayAlert("Eroare", "Toate coordonatele trebuie sa fie numere intregi", "Close");
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            //var result = await Navigation.ShowPopupAsync(new PINOTPPopup());
            var result = await Navigation.ShowPopupAsync(new OTPPopup());

            if(result.ToString() == "UNSET")
            {
                Debug.WriteLine("SALUT");
                return;
            }

            Debug.WriteLine(result.ToString());

            //PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

            //Debug.WriteLine(credObj.PIN);
            //Debug.WriteLine(credObj.OTP);
        }
    }  
}