using LicentaApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp;

namespace SigningApp.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OauthOTPPopup : Popup
    {
        // Source="https://msign-test.transsped.ro/csc/v0/oauth2/authorize?response_type=token&amp;client_id=ts_csc&amp;client_secret=h767ujHG654GHhgI&amp;redirect_uri=http://localhost:8080&amp;scope=credential&amp;credentialID=28A69CAF5B0CC71C4C14CC5B01B639D59227A93D&amp;numSignatures=1&amp;hash=ue3JiD4EmDyEIrrmeVRzxXh1jzne0ZwUd1tqVR7BDwE=&amp;lang=en-US"


        public OauthOTPPopup(string credentialID, int numSignatures, string hash)
        {
            string linkAux = "https://msign-test.transsped.ro/csc/v0/oauth2/authorize?response_type=token&client_id=ts_csc&client_secret=h767ujHG654GHhgI&redirect_uri=http://localhost:8080&scope=credential&" + "credentialID=" + credentialID + "&" + "numSignatures=" + numSignatures.ToString() + "&" + "hash=" + hash + "&lang=en-US";
            linkAux += "credentialID=" + credentialID + "&";
            linkAux += "numSignatures=" + numSignatures.ToString() + "&";
            linkAux += "hash=" + hash + "&lang=en-US";



            InitializeComponent();
            linkAuth.Source = linkAux;
        }

        void webviewNavigating(object sender, WebNavigatingEventArgs e)
        {

            Match m = Regex.Match(e.Url, @"(?i:http://)(?<localhost>[^\s/]*)/\?(?<code>[^\s/]*)\&(?<state>[^\s]*)");
            if (m.Success)
            {
                string servername = m.Groups["localhost"].Value;
                string username = m.Groups["code"].Value;
                string state = m.Groups["state"].Value;

                if (servername == "localhost:8080")
                {
                    string cod = username.Substring(5);

                    //LoginPage.user = new User();

                    bool ok = LoginPage.user.oauth2TokenCredential(cod, "authorization_code");

                    if (ok)
                    {
                        Dismiss("OK");
                    }
                    else
                    {
                        Dismiss("UNSET");
                    }

                    
                }
            }
        }

    }
}