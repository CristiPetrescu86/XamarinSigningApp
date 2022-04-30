using SigningApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SigningApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MultipleXMLSignPage : ContentPage
    {
        public MultipleXMLSignPage()
        {
            InitializeComponent();

            var vm = MultipleXMLSignPageViewModel.Instance;
            this.BindingContext = vm;
            vm.DisplayKeyNotSelected += () => DisplayAlert("Eroare", "Key Not Selected", "Close");
            vm.DisplayFileNotUploaded += () => DisplayAlert("Eroare", "Fisierul nu a fost incarcat", "Close");
            vm.DisplayTipSemnaturaNotChecked += () => DisplayAlert("Eroare", "Tip semnatura nu a fost bifat", "Close");
            vm.DisplayTimestampNotChecked += () => DisplayAlert("Eroare", "Timestamp nu a fost bifat", "Close");
            vm.DisplayPINandOTPnotSet += () => DisplayAlert("Eroare", "PIN sau OTP nu este setat", "Close");
            vm.DisplayPINnotSet += () => DisplayAlert("Eroare", "PIN nu este setat", "Close");
            vm.DisplayOTPnotSet += () => DisplayAlert("Eroare", "OTP nu este setat", "Close");
            vm.DisplayNoCerts += () => DisplayAlert("Eroare", "Nu exista certificat pentru semnatar", "Close");
            vm.DisplayCredAuthNotOK += () => DisplayAlert("Eroare", "Autorizarea cheii nu a putut fi realizata", "Close");
            vm.DisplaySignMethNotOK += () => DisplayAlert("Eroare", "Eroare la semnarea hash-ului", "Close");
            vm.DisplayAlgoNotSelected += () => DisplayAlert("Eroare", "Algoritmul nu a fost selectat", "Close");
        }

        private void LoadAlgos(object sender, EventArgs e)
        {
            var vm = MultipleXMLSignPageViewModel.Instance;
            vm.LoadSignAlgos();
        }

        protected override bool OnBackButtonPressed()
        {
            var vm = MultipleXMLSignPageViewModel.Instance;
            vm.deleteInstance();

            return false;
        }
    }
}