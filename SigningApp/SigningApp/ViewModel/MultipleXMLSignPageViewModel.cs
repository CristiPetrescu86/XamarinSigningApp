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

        public MultipleXMLSignPageViewModel()
        {
            Keys = GetKeys();
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

            LoginPage.user.credentialsInfo(SelectedKey);

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

            foreach (string fileName in DocsPath)
            {
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

                //bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", credObj.PIN, credObj.OTP); // 12345678 123456
                //if (!ok)
                //{
                //    DisplayCredAuthNotOK();
                //    return;
                //}

                //ok = LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
                //if (!ok)
                //{
                //    DisplaySignMethNotOK();
                //    return;
                //}
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

                //bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", pin, null); // 12345678 123456
                //if (!ok)
                //{
                //    DisplayCredAuthNotOK();
                //    return;
                //}

                //ok = LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
                //if (!ok)
                //{
                //    DisplaySignMethNotOK();
                //    return;
                //}
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

                //bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", null, otp); // 12345678 123456
                //if (!ok)
                //{
                //    DisplayCredAuthNotOK();
                //    return;
                //}

                //ok = LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
                //if (!ok)
                //{
                //    DisplaySignMethNotOK();
                //    return;
                //}
            }
            else if (keyObject.OTP.type.Equals("online") && LoginPage.user.authModeSelected.Equals("oauth"))
            {
                var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, hashB64Documents.Count(), hashB64Documents));

                if (result == null)
                {
                    return;
                }

                //string otp = result.ToString();

                //LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", null, otp);
                LoginPage.user.signMultipleHash(SelectedKey, hashB64Documents, "XML");

            }
            else
            {
                //LoginPage.user.credentialsAuthorize(SelectedKey, toBeSigned, "XML", null, null);
                //LoginPage.user.signSingleHash(SelectedKey, toBeSigned, "XML");
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
        }

    }
}