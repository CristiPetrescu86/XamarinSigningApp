using LicentaApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public OauthOTPPopup(string credentialID, int numSignatures, string hash)
        {
            string linkAux = "https://msign-test.transsped.ro/csc/v0/oauth2/authorize?response_type=token&client_id=ts_csc&client_secret=h767ujHG654GHhgI&redirect_uri=http://localhost:8080&scope=credential&";
            linkAux += "credentialID=" + credentialID + "&";
            linkAux += "numSignatures=" + numSignatures.ToString() + "&";
            string hashAux = hash.Replace("+", "-");
            hashAux = hashAux.Replace("/", "_");
            linkAux += "hash=" + hashAux + "&lang=en-US";

            InitializeComponent();
            linkAuth.Source = linkAux;
        }

        public OauthOTPPopup(string credentialID, int numSignatures, List<string> hash)
        {
            string linkAux = "https://msign-test.transsped.ro/csc/v0/oauth2/authorize?response_type=token&client_id=ts_csc&client_secret=h767ujHG654GHhgI&redirect_uri=http://localhost:8080&scope=credential&";
            linkAux += "credentialID=" + credentialID + "&";
            linkAux += "numSignatures=" + numSignatures.ToString() + "&";

            linkAux += "hash="; 
            foreach(string elem in hash)
            {
                string hashAux = elem.Replace("+", "-");
                hashAux = hashAux.Replace("/", "_");
                linkAux += hashAux;
                linkAux += ",";    
            }
            linkAux = linkAux.Remove(linkAux.Length - 1);
            linkAux += "&lang=en-US";

            Debug.WriteLine(linkAux);

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