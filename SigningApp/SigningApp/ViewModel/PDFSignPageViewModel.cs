using LicentaApp.JsonClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using XamarinLicentaApp;

namespace SigningApp.ViewModel
{
    public class PDFSignPageViewModel : ContentView
    {
        public PDFSignPageViewModel()
        {
            Keys = GetKeys();
        }

        private ObservableCollection<string> keys;
        public ObservableCollection<string> Keys
        {
            get { return keys; }
            set { keys = value; }
        }

        private string selectedKey;

        public string SelectedKey
        {
            get { return selectedKey; }
            set { selectedKey = value; }
        }



        private ObservableCollection<string> GetKeys()
        {
            LoginPage.user.credentialsList();

            ObservableCollection<string> newList = new ObservableCollection<string>();

            foreach (string elem in LoginPage.user.credentialsIDs)
            {
                newList.Add(elem);
            }

            return newList;
        }

    }
}