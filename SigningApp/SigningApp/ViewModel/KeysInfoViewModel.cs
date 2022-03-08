using LicentaApp.JsonClass;
using SigningApp.Pages;
using SigningApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Xamarin.Forms;

namespace XamarinLicentaApp.ViewModel
{
    public class KeysInfoViewModel : INotifyPropertyChanged
    {
        public KeysInfoViewModel()
        {
            Keys = GetKeys();
        }

        private CredentialsInfoReceiveClass keySelected;

        public CredentialsInfoReceiveClass KeySelected
        {
            get { return keySelected; }
            set { keySelected = value; }
        }


        private ObservableCollection<CredentialsInfoReceiveClass> keys;
        public ObservableCollection<CredentialsInfoReceiveClass> Keys
        {
            get { return keys; }
            set { keys = value; }
        }


        private ObservableCollection<CredentialsInfoReceiveClass> GetKeys()
        {
            LoginPage.user.credentialsList();
            foreach (string elem in LoginPage.user.credentialsIDs)
            {
                LoginPage.user.credentialsInfo(elem);
            }

            ObservableCollection<CredentialsInfoReceiveClass> newList = new ObservableCollection<CredentialsInfoReceiveClass>();

            foreach (CredentialsInfoReceiveClass elem in LoginPage.user.keysInfo)
            {
                if (elem.key.status == "enabled")
                {
                    elem.key.status = "enabled.png";
                }
                else
                {
                    elem.key.status = "disabled.png";
                }
                newList.Add(elem);
            }

            return newList;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        public async void ShowDetails()
        {
            var newPage = new KeyDetailsPage { BindingContext = new KeyDetailViewModel { KeySelected = KeySelected } };
            await Application.Current.MainPage.Navigation.PushAsync(newPage);
        }

    }

}
       