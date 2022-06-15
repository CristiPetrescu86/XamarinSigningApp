using LicentaApp.JsonClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinLicentaApp;

using iText.Kernel.Pdf;
using LicentaApp;
using Xamarin.Essentials;
using Xamarin.CommunityToolkit.Extensions;
using SigningApp.PopupPages;
using SigningApp.Model;
using SignaturePDF.Library;
using CredentialsInfoReceiveClass = LicentaApp.JsonClass.CredentialsInfoReceiveClass;
using System.IO;
using SignaturePad.Forms;
using System.Threading.Tasks;

namespace SigningApp.ViewModel 
{
    public sealed class PDFSignPageViewModel : ContentView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public Action DisplayKeyNotSelected;
        public Action DisplayXCoordNotSet;
        public Action DisplayYCoordNotSet;
        public Action DisplayWidthNotSet;
        public Action DisplayHeightNotSet;
        public Action DisplayFileNotUploaded;
        public Action DisplayVisibilityNotSet;
        public Action DisplayPageNotSelected;
        public Action DisplayCoordError;
        public Action DisplayPINnotSet;
        public Action DisplayPINandOTPnotSet;
        public Action DisplayOTPnotSet;
        public Action DisplayCredAuthNotOK;
        public Action DisplaySignMethNotOK;
        public Action DisplayTipSemnaturaNotChecked;
        public Action DisplayTimestampNotChecked;
        public Action DisplayAlgoNotSelected;
        public Action DisplaySignNameNotSet;


        Dictionary <string, string> keysAlgo = new Dictionary<string, string>(){
            {"1.2.840.113549.1.1.1", "RSA"},
            {"1.3.14.3.2.29", "RSA-SHA1"},
            {"1.2.840.113549.1.1.14", "RSA-SHA224"},
            {"1.2.840.113549.1.1.11", "RSA-SHA256"},
            {"1.2.840.113549.1.1.12", "RSA-SHA384"}, 
            {"1.2.840.113549.1.1.13", "RSA-SHA512"},
            {"1.2.840.113549.1.1.4", "RSA-MD5"},
            {"RSA", "1.2.840.113549.1.1.1"},
            {"RSA-SHA1", "1.3.14.3.2.29"},
            {"RSA-SHA224", "1.2.840.113549.1.1.14"},
            {"RSA-SHA256", "1.2.840.113549.1.1.11"},
            {"RSA-SHA384", "1.2.840.113549.1.1.12"},
            {"RSA-SHA512", "1.2.840.113549.1.1.13"},
            {"RSA-MD5", "1.2.840.113549.1.1.4"},
            {"SHA224","2.16.840.1.101.3.4.2"},
            {"SHA256","2.16.840.1.101.3.4.2.1"},
            {"SHA-256","2.16.840.1.101.3.4.2.1"},
            {"SHA384","2.16.840.1.101.3.4.2.2"},
            {"SHA512","2.16.840.1.101.3.4.2.3"},
            {"MD5","1.2.840.113549.2.5"},
            {"1.2.840.10045.4.3.2", "ECDSA-SHA256"},
            {"1.2.840.10045.4.3.3", "ECDSA-SHA384"},
            {"1.2.840.10045.4.3.4", "ECDSA-SHA512"}
        };

        private static PDFSignPageViewModel instance = null;
        private static readonly object padlock = new object();

        public PDFSignPageViewModel()
        {
            Keys = GetKeys();
            PageNumber = new ObservableCollection<int>();
        }

        public static PDFSignPageViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new PDFSignPageViewModel();
                    }
                    return instance;
                }
            }
        }

        public void deleteInstance()
        {
            instance = null;
        }


        private ObservableCollection<string> keys;
        public ObservableCollection<string> Keys
        {
            get { return keys; }
            set { keys = value; }
        }

        private ObservableCollection<string> algoForKeys;
        public ObservableCollection<string> AlgoForKeys
        {
            get { return algoForKeys; }
            set { algoForKeys = value; }
        }

        public string SelectedAlgo { get; set; }

        public string SelectedKey { get; set; }

        public string XCoord { get; set; }

        public string YCoord { get; set; }

        public string WidthDist { get; set; }

        public string HeightDist { get; set; }

        public string SelectedType { get; set; }

        public string DocPath { get; set; }

        public int SelectedPage { get; set; }

        public string Motiv { get; set; }

        public string Locatie { get; set; }
        public string NumeSemnatura { get; set; }


        private ObservableCollection<int> pageNumber;

        public string SelectedTimestamp { get; set; }

        public ObservableCollection<int> PageNumber
        {
            get { return pageNumber; }
            set { pageNumber = value; }
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

        public void LoadSignAlgos()
        {
            LoginPage.user.credentialsInfo(SelectedKey);

            CredentialsInfoReceiveClass keyObject = new CredentialsInfoReceiveClass();
            foreach (CredentialsInfoReceiveClass elem in LoginPage.user.keysInfo)
            {
                if (elem.credentialName == SelectedKey)
                {
                    keyObject = elem;
                    break;
                }
            }

            AlgoForKeys = new ObservableCollection<string>();

            string outValue;
            foreach (string elem in keyObject.key.algo)
            {
                keysAlgo.TryGetValue(elem, out outValue);
                AlgoForKeys.Add(outValue);
            }

            PropertyChanged(this, new PropertyChangedEventArgs("AlgoForKeys"));
        }


        private Command signDoc;

        public ICommand SignDoc
        {
            get
            {
                if (signDoc == null)
                {
                    signDoc = new Command(SignPDFButtonClicked);
                }

                return signDoc;
            }
        }

        private Command pickDoc;

        public ICommand PickDoc
        {
            get
            {
                if(pickDoc == null)
                {
                    pickDoc = new Command(PickFile);
                }
                return pickDoc;
            }
        }


        private async void PickFile()
        {
            try
            {
                var file = await FilePicker.PickAsync();

                if (file == null)
                {
                    return;
                }

                DocPath = file.FullPath;

                PdfReader reader2 = new PdfReader(DocPath);
                PdfDocument pdfDocument = new PdfDocument(reader2);
                int pageNumberValue = pdfDocument.GetNumberOfPages();
                pdfDocument.Close();
                reader2.Close();

                for (int i = 1; i <= pageNumberValue; i++)
                {
                    PageNumber.Add(i);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public Func<Task<byte[]>> SignatureFromStream { get; set; }
        public byte[] Signature { get; set; }

        public ICommand GetImageSignature => new Command(async () =>
        {
            Signature = await SignatureFromStream();
        });


        private async void SignPDFButtonClicked()
        {
            if(DocPath == null)
            {
                DisplayFileNotUploaded();
                return;
            }

            if (SelectedKey == null)
            {
                DisplayKeyNotSelected();
                return;
            }

            if(SelectedAlgo == null)
            {
                DisplayAlgoNotSelected();
                return;
            }

            if (SelectedType == null)
            {
                DisplayTipSemnaturaNotChecked();
                return;
            }

            float castedXCoord = 0;
            float castedYCoord = 0;
            float castedWidthDist = 0;
            float castedHeightDist = 0;

            if (SelectedType != "Invizibila")
            {
                try
                {
                    castedXCoord = float.Parse(XCoord);
                    castedYCoord = float.Parse(YCoord);
                    castedWidthDist = float.Parse(WidthDist);
                    castedHeightDist = float.Parse(HeightDist);

                    if (castedXCoord == 0)
                    {
                        DisplayXCoordNotSet();
                        return;
                    }

                    if (castedYCoord == 0)
                    {
                        DisplayYCoordNotSet();
                        return;
                    }

                    if (castedWidthDist == 0)
                    {
                        DisplayWidthNotSet();
                        return;
                    }

                    if (castedHeightDist == 0)
                    {
                        DisplayHeightNotSet();
                        return;
                    }
                }
                catch
                {
                    DisplayCoordError();
                    return;
                }    
            }


            if (SelectedPage < 1 && SelectedType != "Invizibila")
            {
                DisplayPageNotSelected();
                return;
            }

            if (NumeSemnatura == null)
            {
                DisplaySignNameNotSet();
                return;
            }

            if (SelectedTimestamp == null)
            {
                DisplayTimestampNotChecked();
                return;
            }


            //string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.pdf");
            string fileName = DocPath;

            CredentialsInfoReceiveClass keyObject = new CredentialsInfoReceiveClass();
            foreach (CredentialsInfoReceiveClass elem in LoginPage.user.keysInfo)
            {
                if(elem.credentialName == SelectedKey)
                {
                    keyObject = elem;
                    break;
                }
            }

            SignaturePDF.Library.CredentialsInfoReceiveClass toSend = new SignaturePDF.Library.CredentialsInfoReceiveClass();
            toSend.authMode = keyObject.authMode;
            toSend.cert = new SignaturePDF.Library.Certificates();
            toSend.cert.certificates = keyObject.cert.certificates;
            toSend.credentialName = keyObject.credentialName;

            // signature creation

            string signAlgo = SelectedAlgo.GetUntilOrEmpty();
            if (signAlgo == string.Empty)
            {
                signAlgo = SelectedAlgo;
            }
            string hashAlgo = SelectedAlgo.GetAfterOrEmpty();
            if (hashAlgo == string.Empty)
            {
                hashAlgo = "SHA-256";
            }

            GetImageSignature.Execute(null);

            if (Signature == null)
                return;

            ParametersClass parametersClass = new ParametersClass(DocPath, toSend, castedXCoord, castedYCoord, castedWidthDist, castedHeightDist, SelectedPage, Motiv, Locatie, hashAlgo, NumeSemnatura,Signature);

            PDFSignature signature = new PDFSignature();
            byte[] sh = signature.createSign(parametersClass);

            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true" && keyObject.OTP.type == "offline")
            {
                byte[] resultAux = SHAClass.Instance.makeHash(hashAlgo, sh);
                
                if(resultAux == null)
                {
                    // print error
                    return;
                }

                string hashedDocumentB64 = Convert.ToBase64String(resultAux);

                var result = await Navigation.ShowPopupAsync(new PINOTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, hashedDocumentB64, credObj.PIN, credObj.OTP); // 12345678 123456
                if(!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, hashedDocumentB64, signParam, hashParam);
                if(!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if(keyObject.PIN.presence == "true")
            {
                byte[] resultAux = SHAClass.Instance.makeHash(hashAlgo, sh);

                if (resultAux == null)
                {
                    // print error
                    return;
                }

                string hashedDocumentB64 = Convert.ToBase64String(resultAux);

                var result = await Navigation.ShowPopupAsync(new PINPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINnotSet();
                    return;
                }

                string pin = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, hashedDocumentB64, pin, null); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, hashedDocumentB64, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.presence == "true")
            {
                byte[] resultAux = SHAClass.Instance.makeHash(hashAlgo, sh);

                if (resultAux == null)
                {
                    // print error
                    return;
                }

                string hashedDocumentB64 = Convert.ToBase64String(resultAux);

                var result = await Navigation.ShowPopupAsync(new OTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayOTPnotSet();
                    return;
                }

                string otp = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, hashedDocumentB64, null, otp); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }


                ok = LoginPage.user.signSingleHash(SelectedKey, hashedDocumentB64, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if(keyObject.OTP.type.Equals("online") && LoginPage.user.authModeSelected.Equals("oauth"))
            {
                byte[] resultAux = SHAClass.Instance.makeHash(hashAlgo, sh);

                if (resultAux == null)
                {
                    // print error
                    return;
                }

                string hashedDocumentB64 = Convert.ToBase64String(resultAux);

                var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, 1, hashedDocumentB64));

                if (result == null)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if(SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }
                
                bool ok = LoginPage.user.signSingleHash(SelectedKey, hashedDocumentB64, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else
            {
                byte[] resultAux = SHAClass.Instance.makeHash(hashAlgo, sh);

                if (resultAux == null)
                {
                    // print error
                    return;
                }

                string hashedDocumentB64 = Convert.ToBase64String(resultAux);

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }

                bool ok = LoginPage.user.signSingleHash(SelectedKey, hashedDocumentB64, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }

            bool timestampExist = false;
            if (SelectedTimestamp == "Da")
                timestampExist = true;

            bool done = signature.addSignature(Convert.FromBase64String(LoginPage.user.signatures[0]), signAlgo, timestampExist);
            
            if(done)
            {
                // print OK
            }
            else
            {
                // print ERROR
            }

            LoginPage.user.signatures.Clear();
        }

    }

    

}
