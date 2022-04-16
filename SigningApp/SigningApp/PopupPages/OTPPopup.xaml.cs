using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp;

namespace SigningApp.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OTPPopup : Popup
    {
        public OTPPopup()
        {
            InitializeComponent();
        }

        private string credName { get; set; }

        public OTPPopup(string credentialName)
        {
            InitializeComponent();
            credName = credentialName;
        }

        private void Close_Button(object sender, EventArgs e)
        {
            if (otpEntry.Text == null)
            {
                Dismiss("UNSET");
            }

            Dismiss(otpEntry.Text);
        }

        private void Send_OTP(object sender, EventArgs e)
        {
            LoginPage.user.credentialsSendOTP(credName);
        }
    }
}