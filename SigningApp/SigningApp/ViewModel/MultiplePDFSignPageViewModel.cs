using iText.Kernel.Pdf;
using iText.Signatures;
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
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinLicentaApp;

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

        public string SelectedPage { get; set; }

        public string XCoord { get; set; }

        public string YCoord { get; set; }

        public string WidthDist { get; set; }

        public string HeightDist { get; set; }

        public string Motiv { get; set; }

        public string Locatie { get; set; }

        public string CreatorSemnatura { get; set; }

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
                var file = await FilePicker.PickMultipleAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.UWP, new [] {".pdf"} },
                            { DevicePlatform.Android, new [] {".pdf"} },
                            { DevicePlatform.macOS, new [] {".pdf"} },
                            { DevicePlatform.iOS, new [] {".pdf"} }
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

        private async void SignPDFsButtonClicked()
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
                }
            }

            int pageNumber = 0;

            if (SelectedPage == "FirstPage")
            {
                pageNumber = 1;
            }
            else if (SelectedPage == "LastPage")
            {
                pageNumber = 2;
            }
            else
            {
                DisplayPageNotSelected();
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

            List<X509Certificate2> certList = new List<X509Certificate2>();
            foreach (string elem in keyObject.cert.certificates)
            {
                byte[] bytesContent = Convert.FromBase64String(elem);
                certList.Add(new X509Certificate2(bytesContent));
            }

            Org.BouncyCastle.X509.X509Certificate[] certListX509 = new Org.BouncyCastle.X509.X509Certificate[certList.Count];
            int i = 0;
            foreach (X509Certificate2 elem in certList)
            {
                certListX509[i] = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(elem);
                i++;
            }

            // Signature ----------------------------------------------------

            List<byte[]> docContent = new List<byte[]>();
            List<byte[]> docHashes = new List<byte[]>();
            string fieldname = "sig";
            List<string> outTempFileList = new List<string>();

            foreach (string fileName in DocsPath)
            {
                string outTempFileName = Path.GetDirectoryName(fileName);
                Guid myuuid = Guid.NewGuid();
                outTempFileName += @"\";
                outTempFileName += myuuid.ToString();
                outTempFileName += ".TEMP";
                outTempFileList.Add(outTempFileName);


                FileStream outFile2 = new FileStream(outTempFileName, FileMode.Create);
                PdfReader reader = new PdfReader(fileName);
                PdfSigner signer = new PdfSigner(reader, outFile2, new StampingProperties());

                PdfSignatureAppearance appearance2 = signer.GetSignatureAppearance();
                if (SelectedType == "Invizibila")
                {
                    appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(0, 0, 0, 0));
                }
                else if (SelectedType == "Vizibila")
                {
                    appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(castedXCoord, castedYCoord, castedWidthDist, castedHeightDist));
                }
                else
                {
                    DisplayTipSemnaturaNotChecked();
                    return;
                }

                if (pageNumber == 1)
                {
                    appearance2.SetPageNumber(1);
                }
                else
                {
                    PdfDocument pdfDocument = new PdfDocument(reader);
                    int pageNumberValue = pdfDocument.GetNumberOfPages();
                    pdfDocument.Close();

                    appearance2.SetPageNumber(pageNumberValue);
                }
                appearance2.SetCertificate(certListX509[0]);

                if (Motiv != null)
                    appearance2.SetReason(Motiv);

                if (Locatie != null)
                    appearance2.SetLocation(Locatie);

                appearance2.SetSignatureCreator("iTextSharp7 with Bounty Castle");

                if (CreatorSemnatura != null)
                    appearance2.SetContact(CreatorSemnatura);

                signer.SetFieldName(fieldname);

                PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED);
                signer.SignExternalContainer(external, 16000);
                byte[] documentHash = external.getHash();
                docContent.Add(documentHash);

                outFile2.Dispose();
                outFile2.Close();

                PdfPKCS7 sgn = new PdfPKCS7(null, certListX509, "SHA-256", false);
                byte[] sh = sgn.GetAuthenticatedAttributeBytes(documentHash, PdfSigner.CryptoStandard.CADES, null, null);

                docHashes.Add(sh);
            }



            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true" && keyObject.OTP.type == "offline")
            {
                var result = await Navigation.ShowPopupAsync(new PINOTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, docHashes[0], "PDF", credObj.PIN, credObj.OTP); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, docHashes[0], "PDF");
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

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, docHashes[0], "PDF", pin, null); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, docHashes[0], "PDF");
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

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, docHashes[0], "PDF", null, otp); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, docHashes[0], "PDF");
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if (keyObject.OTP.type.Equals("online") && LoginPage.user.authModeSelected.Equals("oauth"))
            {
                List<string> docToBeSigned = new List<string>();
                foreach (byte[] sh in docHashes)
                {
                    SHA256 shaM = new SHA256Managed();
                    var resultAux = shaM.ComputeHash(sh);

                    string hashedDocumentB64 = Convert.ToBase64String(resultAux);
                    docToBeSigned.Add(hashedDocumentB64);
                }
                
                var result = await Navigation.ShowPopupAsync(new OauthOTPPopup(SelectedKey, docHashes.Count, docToBeSigned));

                if (result == null)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                LoginPage.user.signMultipleHash(SelectedKey, docToBeSigned, "PDF");
            }
            else
            {
                //LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", null, null);
                //LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");
            }

            int j = 0;
            foreach (string fileName in DocsPath)
            {
                PdfPKCS7 sgn = new PdfPKCS7(null, certListX509, "SHA-256", false);
                byte[] sh = sgn.GetAuthenticatedAttributeBytes(docContent[j], PdfSigner.CryptoStandard.CADES, null, null);

                sgn.SetExternalDigest(Convert.FromBase64String(LoginPage.user.signatures[j]), null, "RSA");
                
                byte[] encodedSig = null;

                if (SelectedTimestamp == "Da")
                {
                    ITSAClient tsa = new TSAClientBouncyCastle("http://timestamp.digicert.com");
                    //signer.Timestamp(tsa, "SignatureTimestamp");

                    encodedSig = sgn.GetEncodedPKCS7(docContent[j], PdfSigner.CryptoStandard.CADES, tsa, null, null);
                }
                else if (SelectedTimestamp == "Nu")
                {
                    encodedSig = sgn.GetEncodedPKCS7(docContent[j], PdfSigner.CryptoStandard.CADES, null, null, null);
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

                PdfReader reader3 = new PdfReader(outTempFileList[j]);
                PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

                IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

                PdfSigner.SignDeferred(signer3.GetDocument(), fieldname, outFile3, signature3);
                outFile3.Dispose();
                outFile3.Close();

                j++;
            }
            LoginPage.user.signatures.Clear();


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