using LicentaApp.JsonClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Xamarin.Forms;

namespace SigningApp.ViewModel
{
    public class KeyDetailViewModel : INotifyPropertyChanged
    {
        private DateTime valideFrom;
        private DateTime valideTo;

        public DateTime ValideFrom
        {
            get { return valideFrom; }
            set { valideFrom = value; }
        }

        public DateTime ValideTo
        {
            get { return valideTo; }
            set { valideTo = value; }
        }


        private CredentialsInfoReceiveClass keySelected;

        public CredentialsInfoReceiveClass KeySelected
        {
            get { return keySelected; }
            set { keySelected = value; }
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

    }
        
}