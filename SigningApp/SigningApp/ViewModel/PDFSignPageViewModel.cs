using LicentaApp.JsonClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinLicentaApp;

using iText.Kernel.Pdf;
using iText.Signatures;
using LicentaApp;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.CommunityToolkit.Extensions;
using SigningApp.PopupPages;
using SigningApp.Model;

namespace SigningApp.ViewModel 
{
    public class PDFSignPageViewModel : ContentView, INotifyPropertyChanged
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


        public PDFSignPageViewModel()
        {
            Keys = GetKeys();
            PageNumber = new ObservableCollection<int>();
        }

        private ObservableCollection<string> keys;
        public ObservableCollection<string> Keys
        {
            get { return keys; }
            set { keys = value; }
        }

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
        public string CreatorSemnatura { get; set; }



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
                // pickMultipleAsync pentru mai tarziu
                var file = await FilePicker.PickAsync(
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

                if(file == null)
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


        private async void SignPDFButtonClicked()
        {
            //LoginPage.user.credentialsList();
            //LoginPage.user.credentialsInfo(LoginPage.user.credentialsIDs[1]);

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

            if (SelectedPage < 1)
            {
                DisplayPageNotSelected();
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

            string outTempFileName = Path.GetDirectoryName(DocPath);
            Guid myuuid = Guid.NewGuid();
            outTempFileName += @"\";
            outTempFileName += myuuid.ToString();
            outTempFileName += ".TEMP";
            Debug.WriteLine(outTempFileName);

            string fieldname = "sig";

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

            appearance2.SetPageNumber(SelectedPage);
            appearance2.SetCertificate(certListX509[0]);

            if(Motiv != null)
                appearance2.SetReason(Motiv);

            if(Locatie != null)
                appearance2.SetLocation(Locatie);
            
            appearance2.SetSignatureCreator("iTextSharp7 with Bounty Castle");
            
            if(CreatorSemnatura != null)
                appearance2.SetContact(CreatorSemnatura);

            signer.SetFieldName(fieldname);

            PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED);
            signer.SignExternalContainer(external, 16000);
            byte[] documentHash = external.getHash();          

            outFile2.Dispose();
            outFile2.Close();

            PdfPKCS7 sgn = new PdfPKCS7(null, certListX509, "SHA-256", false);
            byte[] sh = sgn.GetAuthenticatedAttributeBytes(documentHash, PdfSigner.CryptoStandard.CADES, null, null);

            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true")
            {
                var result = await Navigation.ShowPopupAsync(new PINOTPPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINandOTPnotSet();
                    return;
                }

                PINandOTP credObj = System.Text.Json.JsonSerializer.Deserialize<PINandOTP>(result.ToString());

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", credObj.PIN, credObj.OTP); // 12345678 123456
                if(!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");
                if(!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else if(keyObject.PIN.presence == "true")
            {
                var result = await Navigation.ShowPopupAsync(new PINPopup());

                if (result.ToString() == "UNSET")
                {
                    DisplayPINnotSet();
                    return;
                }

                string pin = result.ToString();

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", pin, null); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");
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

                bool ok = LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", null, otp); // 12345678 123456
                if (!ok)
                {
                    DisplayCredAuthNotOK();
                    return;
                }

                ok = LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");
                if (!ok)
                {
                    DisplaySignMethNotOK();
                    return;
                }
            }
            else
            {
                LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", null, null);
                LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");
            }

            //LoginPage.user.credentialsAuthorize(SelectedKey, sh, "PDF", "12345678", "123456");
            //LoginPage.user.signSingleHash(SelectedKey, sh, "PDF");

            sgn.SetExternalDigest(Convert.FromBase64String(LoginPage.user.signatures[0]), null, "RSA");


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

            string outFileName3 = Path.GetDirectoryName(DocPath);
            outFileName3 += @"\";
            outFileName3 += Path.GetFileNameWithoutExtension(DocPath);
            outFileName3 += "SIGNED";
            outFileName3 += Path.GetExtension(DocPath);

            FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

            PdfReader reader3 = new PdfReader(outTempFileName);
            PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

            IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

            PdfSigner.SignDeferred(signer3.GetDocument(), fieldname, outFile3, signature3);
            outFile3.Dispose();
            outFile3.Close();

            //if (File.Exists(outTempFileName))
            //{
            //    File.Delete(outTempFileName);
            //}
        }

    }
    public class MyExternalSignatureContainer : IExternalSignatureContainer
    {

        protected byte[] sig;

        public MyExternalSignatureContainer(byte[] sig)
        {
            this.sig = sig;
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            throw new NotImplementedException();
        }

        byte[] IExternalSignatureContainer.Sign(Stream data)
        {
            return sig;
        }
    }

    public class PreSignatureContainer : IExternalSignatureContainer
    {
        private PdfDictionary sigDic;
        private byte[] hash;

        public PreSignatureContainer(PdfName filter, PdfName subFilter)
        {
            sigDic = new PdfDictionary();
            sigDic.Put(PdfName.Filter, filter);
            sigDic.Put(PdfName.SubFilter, subFilter);
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            signDic.PutAll(sigDic);
        }

        public byte[] Sign(Stream data)
        {
            string hashAlgorithm = "SHA256";

            try
            {
                this.hash = DigestAlgorithms.Digest(data, hashAlgorithm);
            }
            catch (IOException e)
            {
                throw new GeneralSecurityException("PreSignatureContainer signing exception", e);
            }

            return new byte[0];
        }

        public byte[] getHash()
        {
            return hash;
        }
    }
}
