using LicentaApp.JsonClass;
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
using System.Windows.Input;
using System.Xml;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinLicentaApp;
using SignatureXML.Library;
using CredentialsInfoReceiveClass = LicentaApp.JsonClass.CredentialsInfoReceiveClass;

namespace SigningApp.ViewModel
{
    public class MultipleXMLSignPageViewModel : ContentView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public Action DisplayKeyNotSelected;
        public Action DisplayFileNotUploaded;
        public Action DisplayTipSemnaturaNotChecked;
        public Action DisplayTimestampNotChecked;
        public Action DisplayPINandOTPnotSet;
        public Action DisplayPINnotSet;
        public Action DisplayOTPnotSet;
        public Action DisplayNoCerts;
        public Action DisplayCredAuthNotOK;
        public Action DisplaySignMethNotOK;
        public Action DisplayAlgoNotSelected;

        readonly Dictionary<string, string> keysAlgo = new Dictionary<string, string>(){
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

        public string SelectedKey { get; set; }

        public string SelectedType { get; set; }

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

        private static MultipleXMLSignPageViewModel instance = null;
        private static readonly object padlock = new object();

        public static MultipleXMLSignPageViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MultipleXMLSignPageViewModel();
                    }
                    return instance;
                }
            }
        }

        public void deleteInstance()
        {
            instance = null;
        }

        public MultipleXMLSignPageViewModel()
        {
            Keys = GetKeys();
        }

        private ObservableCollection<string> algoForKeys;
        public ObservableCollection<string> AlgoForKeys
        {
            get { return algoForKeys; }
            set { algoForKeys = value; }
        }

        public string SelectedAlgo { get; set; }

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
                    signDocs = new Command(SignXMLsButtonClicked);
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

        private async void SignXMLsButtonClicked()
        {
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


            CredentialsInfoReceiveClass keyObject = new CredentialsInfoReceiveClass();
            foreach (CredentialsInfoReceiveClass elem in LoginPage.user.keysInfo)
            {
                if (elem.credentialName == SelectedKey)
                {
                    keyObject = elem;
                    break;
                }
            }

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

            SignatureXML.Library.CredentialsInfoReceiveClass toSend = new SignatureXML.Library.CredentialsInfoReceiveClass();
            toSend.authMode = keyObject.authMode;
            toSend.cert = new SignatureXML.Library.Certificates();
            toSend.cert.certificates = keyObject.cert.certificates;
            toSend.credentialName = keyObject.credentialName;

            bool selectedType = false;
            if (SelectedType == "XAdES")
                selectedType = true;

            ConfigureMultipleClass detailClass = new ConfigureMultipleClass(DocsPath, toSend, hashAlgo, signAlgo, selectedType, SelectedAlgo);

            XMLSignature signature = new XMLSignature();
            List<string> hashB64Documents = signature.getMultipleSignatureContent(detailClass);

            string signParam = null;
            string hashParam = null;
            keysAlgo.TryGetValue(SelectedAlgo, out signParam);
            if (SelectedAlgo == "RSA")
            {
                keysAlgo.TryGetValue(hashAlgo, out hashParam);
            }

            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true")
            {
                var result = await Navigation.ShowPopupAsync(new PINOTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

                bool ok = LoginPage.user.credentialsAuthorizeMultipleHash(SelectedKey, hashB64Documents, credObj.PIN, credObj.OTP, hashB64Documents.Count); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.PIN.presence == "true")
            {
                var result = await Navigation.ShowPopupAsync(new PINPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINnotSet();
                    return;
                }

                string pin = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorizeMultipleHash(SelectedKey, hashB64Documents, pin, null, hashB64Documents.Count); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.presence == "true")
            {
                var result = await Navigation.ShowPopupAsync(new OTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayOTPnotSet();
                    return;
                }

                string otp = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorizeMultipleHash(SelectedKey, hashB64Documents, null, otp, hashB64Documents.Count); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.type.Equals("online") && LoginPage.user.authModeSelected.Equals("oauth"))
            {
                var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, hashB64Documents.Count(), hashB64Documents));

                if (result == null)
                {
                    return;
                }

                bool ok = LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else
            {
                bool ok = LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }

            signature.attachMultipleSignatureToDoc(LoginPage.user.signatures);

            LoginPage.user.signatures.Clear();
        }
    }
}