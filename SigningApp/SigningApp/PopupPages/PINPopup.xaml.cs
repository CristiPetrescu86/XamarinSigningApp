using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SigningApp.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PINPopup : Popup
    {
        public PINPopup()
        {
            InitializeComponent();
        }

        private void Close_Button(object sender, EventArgs e)
        {
            if (pinEntry.Text == null)
            {
                Dismiss("UNSET");
            }

            Dismiss(pinEntry.Text);
        }

    }
}