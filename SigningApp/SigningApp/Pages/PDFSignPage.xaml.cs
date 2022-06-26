﻿using SigningApp.Model;
using SigningApp.PopupPages;
using SigningApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
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
            vm.DisplayAlgoNotSelected += () => DisplayAlert("Eroare", "Algoritmul nu a fost selectat", "Close");
            vm.DisplaySignNameNotSet += () => DisplayAlert("Eroare", "Numele semnaturii nu a fost introdus", "Close");
            var messageOptions = new MessageOptions
            {
                Message = "Signature done successful!",
                Foreground = Color.White,
                Font = Font.SystemFontOfSize(16),
                Padding = new Thickness(20),
            };
            var options = new ToastOptions
            {
                MessageOptions = messageOptions,
                CornerRadius = new Thickness(40, 40, 0, 0),
                BackgroundColor = Color.FromHex("#34495E")
            };
            vm.DisplaySignatureDone += () => this.DisplayToastAsync(options);

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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = (PDFSignPageViewModel)BindingContext; // Warning, the BindingContext View <-> ViewModel is already set

            vm.SignatureFromStream = async () =>
            {
                if (SignatureView.Points.Count() > 0)
                {
                    using (var stream = await SignatureView.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png))
                    {
                        return await ImageConverter.ReadFully(stream);
                    }
                }

                return await Task.Run(() => (byte[])null);
            };
        }

        public static class ImageConverter
        {
            public static async Task<byte[]> ReadFully(Stream input)
            {
                byte[] buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }
    }  
}