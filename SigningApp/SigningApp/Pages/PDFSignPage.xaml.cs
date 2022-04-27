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

            var vm = PDFSignPageViewModel.Instance;
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
            vm.DisplayPINandOTPnotSet += () => DisplayAlert("Eroare", "PIN sau OTP nu este setat","Close");
            vm.DisplayPINnotSet += () => DisplayAlert("Eroare", "PIN nu este setat", "Close");
            vm.DisplayOTPnotSet += () => DisplayAlert("Eroare", "OTP nu este setat", "Close");
            vm.DisplayCredAuthNotOK += () => DisplayAlert("Eroare", "Autorizarea cheii nu a putut fi realizata", "Close");
            vm.DisplaySignMethNotOK += () => DisplayAlert("Eroare", "Eroare la semnarea hash-ului", "Close");
            vm.DisplayTipSemnaturaNotChecked += () => DisplayAlert("Eroare", "Tip semnatura nu a fost bifat", "Close");
            vm.DisplayTimestampNotChecked += () => DisplayAlert("Eroare", "Timestamp nu a fost bifat", "Close");
        }

        private void LoadAlgos(object sender, EventArgs e)
        {
            var vm = PDFSignPageViewModel.Instance;
            vm.LoadSignAlgos();
        }

        protected override bool OnBackButtonPressed()
        {
            var vm = PDFSignPageViewModel.Instance;
            vm.deleteInstance();

            return false;
        }
    }  
}