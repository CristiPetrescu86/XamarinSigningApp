using LicentaApp.JsonClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XamarinLicentaApp.ViewModel
{
    public class KeysInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private List<string> keyName = new List<string>();

        public List<string> KeyName
        {
            get { return keyName; }
        }


        private CredentialsInfoReceiveClass key;
        public CredentialsInfoReceiveClass Key
        {
            get { return key; }
            set
            {
                if(key != value)
                {
                    key = value;
                }
            }
        }
    }
}