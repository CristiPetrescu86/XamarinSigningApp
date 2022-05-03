using LicentaApp.JsonClass;
using SigningApp.Model;
using SigningApp.PopupPages;
using SigningApp.XadesSignedXML.XML;
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
                var file = await FilePicker.PickMultipleAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.UWP, new [] {".xml"} },
                            { DevicePlatform.Android, new [] { ".xml" } },
                            { DevicePlatform.macOS, new [] { ".xml" } },
                            { DevicePlatform.iOS, new [] { ".xml" } }
                        })
                    });

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

            List<string> hashB64Documents = new List<string>();
            List<XmlDocument> xmlDocs = new List<XmlDocument>();
            List<SignedXml> signedDocs = new List<SignedXml>();

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

            List<X509Certificate2> certList = new List<X509Certificate2>();
            foreach (string elem in keyObject.cert.certificates)
            {
                byte[] bytes = Convert.FromBase64String(elem);
                certList.Add(new X509Certificate2(bytes));
            }

            if (certList == null)
            {
                DisplayNoCerts();
                return;
            }

            foreach (string fileName in DocsPath)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                Guid myuuid = Guid.NewGuid();
                string myuuidAsString = "xmldsig-" + myuuid.ToString();

                SignedXml signedXml = new SignedXml(doc);
                signedXml.PublicKeyCert = certList[0].PublicKey.Key;
                signedXml.Signature.Id = myuuidAsString;
                Reference reference = new Reference();
                if (hashAlgo == "SHA256" || hashAlgo == "SHA-256")
                    reference.DigestMethod = SignedXml.XmlDsigSHA256Url;
                else if (hashAlgo == "SHA1")
                    reference.DigestMethod = SignedXml.XmlDsigSHA1Url;
                else if (hashAlgo == "SHA384")
                    reference.DigestMethod = SignedXml.XmlDsigSHA384Url;
                else if (hashAlgo == "SHA512")
                    reference.DigestMethod = SignedXml.XmlDsigSHA512Url;

                reference.Uri = "";
                XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(env);
                signedXml.AddReference(reference);


                KeyInfo keyInfo = new KeyInfo();
                KeyInfoX509Data dataCerts = new KeyInfoX509Data();
                foreach (X509Certificate2 elem in certList)
                {
                    dataCerts.AddCertificate(elem);
                }
                keyInfo.AddClause(dataCerts);


                signedXml.KeyInfo = keyInfo;

                byte[] toBeSigned;

                if (SelectedType == "XAdES")
                {
                    XadesObject xo = new XadesObject();
                    {
                        List<Cert> certAuxList = new List<Cert>();

                        foreach (X509Certificate2 elem in certList)
                        {
                            Cert certAux = new Cert();

                            certAux.IssuerSerial.X509IssuerName = elem.IssuerName.Name;
                            certAux.IssuerSerial.X509SerialNumber = elem.SerialNumber;
                            {
                                SHA256 cryptoServiceProvider = new SHA256CryptoServiceProvider();
                                certAux.CertDigest.DigestValue = cryptoServiceProvider.ComputeHash(elem.RawData);
                                certAux.CertDigest.DigestMethod.Algorithm = SignedXml.XmlDsigSHA256Url;
                            }

                            certAuxList.Add(certAux);
                        }

                        xo.QualifyingProperties.Target = "#" + signedXml.Signature.Id;
                        xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningTime = DateTime.Now;
                        xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyImplied = true;

                        foreach (Cert elem in certAuxList)
                        {
                            xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningCertificate.CertCollection.Add(elem);
                        }


                        //DataObjectFormat dof = new DataObjectFormat();
                        //dof.ObjectReferenceAttribute = "#Document";
                        //dof.Description = "Document xml[XML]";
                        //dof.Encoding = SignedXml.XmlDsigBase64TransformUrl;
                        //dof.MimeType = "text/plain";
                        //xo.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection.Add(dof);
                    }
                    signedXml.AddXadesObject(xo);
                }

                if (SelectedAlgo == "RSA-SHA1")
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
                else if (SelectedAlgo == "RSA-SHA256")
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
                else if (SelectedAlgo == "RSA-SHA384")
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA384Url;
                else if (SelectedAlgo == "RSA-SHA512")
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA512Url;
                else if (signAlgo == "RSA" && hashAlgo == "SHA-256")
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

                toBeSigned = signedXml.ComputeSignature();

                hashB64Documents.Add(Convert.ToBase64String(toBeSigned));

                signedDocs.Add(signedXml);
                xmlDocs.Add(doc);
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

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
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

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
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

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
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

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
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
                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }

                bool ok = LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, signParam, hashParam);
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }


            for (int j = 0; j < xmlDocs.Count(); j++)
            {
                signedDocs[j].setSignatureValue(LoginPage.user.signatures[j]);

                XmlElement xmlSig = signedDocs[j].GetXml();

                xmlDocs[j].DocumentElement.AppendChild(xmlDocs[j].ImportNode(xmlSig, true));

                string outFileName = Path.GetDirectoryName(DocsPath[j]);
                outFileName += @"\";
                outFileName += Path.GetFileNameWithoutExtension(DocsPath[j]);
                outFileName += "SIGNED";
                outFileName += Path.GetExtension(DocsPath[j]);

                //doc.Save(outFileName);
                File.WriteAllText(outFileName, xmlDocs[j].OuterXml);
            }

            LoginPage.user.signatures.Clear();
        }

    }
}