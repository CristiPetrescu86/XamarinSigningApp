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
            DateTime validFrom = new DateTime(int.Parse(KeySelected.cert.validFrom.Substring(0, 4)), int.Parse(KeySelected.cert.validFrom.Substring(4, 2)), int.Parse(KeySelected.cert.validFrom.Substring(6, 2)), int.Parse(KeySelected.cert.validFrom.Substring(8, 2)), int.Parse(KeySelected.cert.validFrom.Substring(10, 2)), int.Parse(KeySelected.cert.validFrom.Substring(12, 2)));
            DateTime validTo = new DateTime(int.Parse(KeySelected.cert.validTo.Substring(0, 4)), int.Parse(KeySelected.cert.validTo.Substring(4, 2)), int.Parse(KeySelected.cert.validTo.Substring(6, 2)), int.Parse(KeySelected.cert.validTo.Substring(8, 2)), int.Parse(KeySelected.cert.validTo.Substring(10, 2)), int.Parse(KeySelected.cert.validTo.Substring(12, 2)));

            var newPage = new KeyDetailsPage { BindingContext = new KeyDetailViewModel { KeySelected = KeySelected, ValideFrom = validFrom, ValideTo = validTo } };
            await Application.Current.MainPage.Navigation.PushAsync(newPage);
        }

    }

}
       