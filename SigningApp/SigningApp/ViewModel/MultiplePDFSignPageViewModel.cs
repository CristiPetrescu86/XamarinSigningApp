using iText.Kernel.Pdf;
using iText.Signatures;
using LicentaApp;
using LicentaApp.JsonClass;
using SignaturePDF.Library;
using SigningApp.Model;
using SigningApp.PopupPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinLicentaApp;
using CredentialsInfoReceiveClass = LicentaApp.JsonClass.CredentialsInfoReceiveClass;

namespace SigningApp.ViewModel
{
    public class MultiplePDFSignPageViewModel : ContentView, INotifyPropertyChanged
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
        public Action DisplaySignatureDone;
        public Action DisplayXbig;
        public Action DisplayYBig;
        public Action DisplayTooLargeWidth;
        public Action DisplayTooLargeHeight;

        Dictionary<string, string> keysAlgo = new Dictionary<string, string>(){
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

        private List<string> docsPath;

        public List<string> DocsPath
        {
            get { return docsPath; }
            set { docsPath = value; }
        }

        private static MultiplePDFSignPageViewModel instance = null;
        private static readonly object padlock = new object();

        public static MultiplePDFSignPageViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MultiplePDFSignPageViewModel();
                    }
                    return instance;
                }
            }
        }

        public void deleteInstance()
        {
            instance = null;
        }

        private List<string> GetDocsPath(IEnumerable<FileResult> collection)
        {
            List<string> newList = new List<string>();

            foreach (FileResult elem in collection)
            {
                newList.Add(elem.FullPath);
            }

            return newList;
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

        public string SelectedType { get; set; }

        public string SelectedPage { get; set; }

        public string XCoord { get; set; }

        public string YCoord { get; set; }

        public string WidthDist { get; set; }

        public string HeightDist { get; set; }

        public string Motiv { get; set; }

        public string Locatie { get; set; }

        public string NumeSemnatura { get; set; }

        public string SelectedTimestamp { get; set; }

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

        public MultiplePDFSignPageViewModel()
        {
            Keys = GetKeys();
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


        private Command signDocs;

        public ICommand SignDocs
        {
            get
            {
                if (signDocs == null)
                {
                    signDocs = new Command(SignPDFsButtonClicked);
                }

                return signDocs;
            }
        }

        private Command pickDocs;

        public ICommand PickDocs
        {
            get
            {
                if (pickDocs == null)
                {
                    pickDocs = new Command(PickFiles);
                }
                return pickDocs;
            }
        }


        private async void PickFiles()
        {
            try
            {
                // pickMultipleAsync pentru mai tarziu
                var file = await FilePicker.PickMultipleAsync();

                if (file == null)
                {
                    return;
                }

                DocsPath = GetDocsPath(file);


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
            //Signature = SignatureFromStream().GetAwaiter().GetResult();
        });


        private Command getSignWrite;

        public ICommand GetSignWrite
        {
            get
            {
                if (getSignWrite == null)
                {
                    getSignWrite = new Command(getSignWriteFunction);
                }
                return getSignWrite;
            }
        }

        void getSignWriteFunction()
        {
            GetImageSignature.Execute(null);
        }

        private async void SignPDFsButtonClicked()
        {
            GetImageSignature.Execute(null);

            if (DocsPath == null)
            {
                DisplayFileNotUploaded();
                return;
            }

            if (SelectedKey == null)
            {
                DisplayKeyNotSelected();
                return;
            }

            if (SelectedAlgo == null)
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

                    if (castedXCoord >= 595 || castedXCoord < 0)
                    {
                        DisplayXbig();
                        return;
                    }

                    if (castedXCoord + castedWidthDist >= 595)
                    {
                        DisplayTooLargeWidth();
                        return;
                    }

                    if (castedYCoord >= 840 || castedYCoord < 0)
                    {
                        DisplayYBig();
                        return;
                    }

                    if (castedYCoord + castedHeightDist >= 840)
                    {
                        DisplayTooLargeHeight();
                        return;
                    }

                }
                catch
                {
                    DisplayCoordError();
                    return;
                }
            }

            int pageNumber = 0;

            if (SelectedPage == null && SelectedType != "Invizibila")
            {
                DisplayPageNotSelected();
                return;
            }
            else
            {
                if (SelectedPage == "FirstPage")
                {
                    pageNumber = 1;
                }
                else if (SelectedPage == "LastPage")
                {
                    pageNumber = 2;
                }
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

            CredentialsInfoReceiveClass keyObject = new CredentialsInfoReceiveClass();
            foreach (CredentialsInfoReceiveClass elem in LoginPage.user.keysInfo)
            {
                if (elem.credentialName == SelectedKey)
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

            ParametersMultipleClass parametersClass = new ParametersMultipleClass(DocsPath, toSend, castedXCoord, castedYCoord, castedWidthDist, castedHeightDist, pageNumber, Motiv, Locatie, hashAlgo, NumeSemnatura, Signature);

            PDFSignature signature = new PDFSignature();
            List<byte[]> docHashes = signature.createMultipleSign(parametersClass);

            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true" && keyObject.OTP.type == "offline")
            {
                List<string> docToBeSigned = SHAClass.Instance.makeHashesB64(hashAlgo,docHashes);

                var result = await Navigation.ShowPopupAsync(new PINOTPPopup());

                if (result == null)
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                if (result.ToString() == "UNSET")
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

                bool ok = LoginPage.user.credentialsAuthorizeMultipleHash(SelectedKey, docToBeSigned, credObj.PIN, credObj.OTP, docToBeSigned.Count); // 12345678 123456
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

                ok = LoginPage.user.signMultipleHash(SelectedKey, docToBeSigned, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.PIN.presence == "true")
            {
                List<string> docToBeSigned = SHAClass.Instance.makeHashesB64(hashAlgo, docHashes);

                var result = await Navigation.ShowPopupAsync(new PINPopup());

                if (result == null)
                {
                    DisplayPINnotSet();
                    return;
                }

                if (result.ToString() == "UNSET")
                {
                    DisplayPINnotSet();
                    return;
                }

                string pin = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorizeMultipleHash(SelectedKey, docToBeSigned, pin,null, docToBeSigned.Count); // 12345678 123456
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

                ok = LoginPage.user.signMultipleHash(SelectedKey, docToBeSigned, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.presence == "true")
            {
                List<string> docToBeSigned = SHAClass.Instance.makeHashesB64(hashAlgo, docHashes);

                var result = await Navigation.ShowPopupAsync(new OTPPopup());

                if (result == null)
                {
                    DisplayOTPnotSet();
                    return;
                }

                if (result.ToString() == "UNSET")
                {
                    DisplayOTPnotSet();
                    return;
                }

                string otp = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorizeMultipleHash(SelectedKey, docToBeSigned, null, otp, docToBeSigned.Count); // 12345678 123456
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

                ok = LoginPage.user.signMultipleHash(SelectedKey, docToBeSigned, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.type.Equals("online") && LoginPage.user.authModeSelected.Equals("oauth"))
            {
                List<string> docToBeSigned = SHAClass.Instance.makeHashesB64(hashAlgo, docHashes);

                var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, docToBeSigned.Count, docToBeSigned));

                if (result == null)
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

                bool ok = LoginPage.user.signMultipleHash(SelectedKey, docToBeSigned, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else
            {
                List<string> docToBeSigned = SHAClass.Instance.makeHashesB64(hashAlgo, docHashes);

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }

                bool ok = LoginPage.user.signMultipleHash(SelectedKey, docToBeSigned, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }

            bool timestampExist = false;
            if (SelectedTimestamp == "Da")
                timestampExist = true;

            List<byte[]> decB64hashes = new List<byte[]>();
            foreach(string el in LoginPage.user.signatures)
            {
                decB64hashes.Add(Convert.FromBase64String(el));
            }
            bool done = signature.addMultipleSignature(decB64hashes, signAlgo, timestampExist);

            LoginPage.user.signatures.Clear();

            DisplaySignatureDone();

            /*else
            {
                LoginPage.user.credentialsExtendTransaction(sh, SelectedKey,"PDF");
                //LoginPage.user.oauth2TokenCredentialRenew("authorization_code");
                LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");


                sgn.SetExternalDigest(Convert.FromBase64String(LoginPage.user.signatures[0]), null, "RSA");
                LoginPage.user.signatures.Clear();

                byte[] encodedSig = null;

                if (SelectedTimestamp == "Da")
                {
                    ITSAClient tsa = new TSAClientBouncyCastle("http://timestamp.digicert.com");
                    //signer.Timestamp(tsa, "SignatureTimestamp");

                    encodedSig = sgn.GetEncodedPKCS7(documentHash, PdfSigner.CryptoStandard.CADES, tsa, null, null);
                }
                else if (SelectedTimestamp == "Nu")
                {
                    encodedSig = sgn.GetEncodedPKCS7(documentHash, PdfSigner.CryptoStandard.CADES, null, null, null);
                }
                else
                {
                    DisplayTimestampNotChecked();
                    return;
                }

                string outFileName3 = Path.GetDirectoryName(fileName);
                outFileName3 += @"\";
                outFileName3 += Path.GetFileNameWithoutExtension(fileName);
                outFileName3 += "SIGNED";
                outFileName3 += Path.GetExtension(fileName);

                FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

                PdfReader reader3 = new PdfReader(outTempFileName);
                PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

                IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

                PdfSigner.SignDeferred(signer3.GetDocument(), fieldname, outFile3, signature3);
                outFile3.Dispose();
                outFile3.Close();
            }
            */


        }
    }

           
}