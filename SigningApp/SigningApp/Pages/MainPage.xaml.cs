using Plugin.SharedTransitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinLicentaApp.Pages;

namespace XamarinLicentaApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OpenMenu()
        {
            MenuGrid.IsVisible = true;

            Action<double> callback = input => MenuView.TranslationX = input;
            MenuView.Animate("anim", callback, -220, 0, 16, 300, Easing.CubicInOut);
        }

        private void CloseMenu()
        {
            Action<double> callback = input => MenuView.TranslationX = input;
            MenuView.Animate("anim", callback, 0, -220, 16, 300, Easing.CubicInOut);

            MenuGrid.IsVisible = false;
        }


        private void MenuTapped(object sender, EventArgs e)
        {
            OpenMenu();
        }

        private void OverlayTapped(object sender, EventArgs e)
        {
            CloseMenu();
        }

        private async void MenuOptionSelected(object sender, SelectionChangedEventArgs e)
        {
            String type = vm.SelectedMenuOption.Name;
            if(type == "Sign PDF")
            {
                var detailPage = new PDFSignPage();
                await Application.Current.MainPage.Navigation.PushAsync(detailPage);
            }
            else if(type == "Sign XML")
            {  
                await DisplayAlert("salut","salut","OK");
            }
            else if(type == "Keys Info")
            {
                var newPage = new KeysInfo();
                await Application.Current.MainPage.Navigation.PushAsync(newPage);
            }
            else if(type == "Logout")
            {
                await DisplayAlert("salut2", "salut2", "OK2");
            }

            
        }
    }
}
