using SigningApp.ViewModel;
using SigningApp.XadesSignedXML.XML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp;

namespace SigningApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class XMLSignPage : ContentPage
    {
        public XMLSignPage()
        {
            InitializeComponent();

            var vm = new XMLSignPageViewModel();
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
        }

    }
}