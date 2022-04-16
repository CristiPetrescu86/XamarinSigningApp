using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinLicentaApp;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Xml;
using SigningApp.XadesSignedXML.XML;
using LicentaApp.JsonClass;
using Xamarin.CommunityToolkit.Extensions;
using SigningApp.PopupPages;
using SigningApp.Model;
using System.Threading.Tasks;
using System.Threading;

namespace SigningApp.ViewModel
{
    public class XMLSignPageViewModel : ContentView, INotifyPropertyChanged
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

        public XMLSignPageViewModel()
        {
            Keys = GetKeys();
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

        public string DocPath { get; set; }

        private Command pickDoc;

        public ICommand PickDoc
        {
            get
            {
                if (pickDoc == null)
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
                // pickMultipleAsync pentru mai tarziu
                var file = await FilePicker.PickAsync(
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

                DocPath = file.FullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Command signDoc;

        public ICommand SignDoc
        {
            get
            {
                if (signDoc == null)
                {
                    signDoc = new Command(SignXMLButtonClicked);
                }

                return signDoc;
            }
        }

        public string HashedDocumentB64 { get; set; }

        private async void SignXMLButtonClicked()
        {
            if (DocPath == null)
            {
                DisplayFileNotUploaded();
                return;
            }

            if (SelectedKey == null)
            {
                DisplayKeyNotSelected();
                return;
            }

            LoginPage.user.credentialsInfo(SelectedKey);

            if (SelectedType == null)
            {
                DisplayTipSemnaturaNotChecked();
                return;
            }

            string fileName = DocPath;

            CredentialsInfoReceiveClass keyObject = new CredentialsInfoReceiveClass();
            foreach (CredentialsInfoReceiveClass elem in LoginPage.user.keysInfo)
            {
                if (elem.credentialName == SelectedKey)
                {
                    keyObject = elem;
                    break;
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            

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

            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = "xmldsig-" + myuuid.ToString();

            SignedXml signedXml = new SignedXml(doc);
            signedXml.PublicKeyCert = certList[0].PublicKey.Key;
            signedXml.Signature.Id = myuuidAsString;
            Reference reference = new Reference();
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

                    foreach(X509Certificate2 elem in certList)
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
             
            toBeSigned = signedXml.ComputeSignature();
            
            HashedDocumentB64 = Convert.ToBase64String(toBeSigned);

            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true")
            {
                var result = await Navigation.ShowPopupAsync(new PINOTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", credObj.PIN, credObj.OTP); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
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

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", pin, null); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
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

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", null, otp); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.type.Equals("online") && LoginPage.user.authModeSelected.Equals("oauth"))
            {
                //var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, 1, hashedDocumentB64));
                var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, 1, HashedDocumentB64));

                if (result == null)
                {
                    return;
                }

                //string otp = result.ToString();

                //LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", null, otp);
                LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");

            }
            else
            {
                LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", null, null);
                LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
            }

            //LoginPage.user.credentialsAuthorize(LoginPage.user.credentialsIDs[1], toBeSigned, "XML", "12345678", "123456");
            //LoginPage.user.signSingleHash(LoginPage.user.credentialsIDs[1], toBeSigned, "XML");

            signedXml.setSignatureValue(LoginPage.user.signatures[0]);

            XmlElement xmlSig = signedXml.GetXml();

            doc.DocumentElement.AppendChild(doc.ImportNode(xmlSig, true));

            string outFileName = Path.GetDirectoryName(DocPath);
            outFileName += @"\";
            outFileName += Path.GetFileNameWithoutExtension(DocPath);
            outFileName += "SIGNED";
            outFileName += Path.GetExtension(DocPath);

            //doc.Save(outFileName);
            File.WriteAllText(outFileName, doc.OuterXml);



            //string filePath3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "my-cert.pem");
            //X509Certificate2 pubCert = new X509Certificate2(filePath3);
            //XmlDocument doc2 = new XmlDocument();
            //doc2.Load(filePath2);
            //SignedXml signedXMLVerif = new SignedXml(doc);
            //XmlNode signNode = doc2.GetElementsByTagName("Signature")[0];
            //signedXml.LoadXml((XmlElement)signNode);
            //bool verif = signedXMLVerif.CheckSignature(pubCert, true);
            //Debug.WriteLine(verif);


        }


    }
}