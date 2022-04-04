using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SigningApp.Model;

namespace SigningApp.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PINOTPPopup : Popup
    {
        public PINOTPPopup()
        {
            InitializeComponent();
        }

        private void Close_Button(object sender, EventArgs e)
        {
            if(pinEntry.Text == null || otpEntry.Text == null)
            {
                Dismiss("UNSET");
            }

            PINandOTP credObject = new PINandOTP(pinEntry.Text, otpEntry.Text);
            string jsonObj = System.Text.Json.JsonSerializer.Serialize(credObject);

            Dismiss(jsonObj);
        }
    }
}