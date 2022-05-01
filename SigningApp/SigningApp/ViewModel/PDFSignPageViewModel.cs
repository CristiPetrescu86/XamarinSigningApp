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
using System.Security.Cryptography;
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
                }    
            }

            if (SelectedPage < 1 && SelectedType != "Invizibila")
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

            FileStream outFile2 = new FileStream(outTempFileName, FileMode.Create);
            PdfReader reader = new PdfReader(fileName);
            PdfSigner signer = new PdfSigner(reader, outFile2, new StampingProperties().UseAppendMode());

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

            if (NumeSemnatura == null)
            {
                DisplaySignNameNotSet();
                return;
            }
            signer.SetFieldName(NumeSemnatura);

            if (SelectedTimestamp == null)
            {
                DisplayTimestampNotChecked();
                return;
            }

            string signAlgo = SelectedAlgo.GetUntilOrEmpty();
            if(signAlgo == string.Empty)
            {
                signAlgo = SelectedAlgo;
            }
            string hashAlgo = SelectedAlgo.GetAfterOrEmpty();
            if(hashAlgo == string.Empty)
            {
                hashAlgo = "SHA-256";
            }

            PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED, hashAlgo);
            signer.SignExternalContainer(external, 16000);
            byte[] documentHash = external.getHash();

            outFile2.Dispose();
            outFile2.Close();


            PdfPKCS7 sgn = new PdfPKCS7(null, certListX509, hashAlgo, false);
            byte[] sh = sgn.GetAuthenticatedAttributeBytes(documentHash, PdfSigner.CryptoStandard.CADES, null, null);

            if (keyObject.PIN.presence == "true" && keyObject.OTP.presence == "true" && keyObject.OTP.type == "offline")
            {
                byte[] resultAux = null;
                if (hashAlgo == "SHA256" || hashAlgo == "SHA-256")
                {
                    SHA256 shaM = new SHA256Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA1")
                {
                    SHA1 shaM = new SHA1Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA384")
                {
                    SHA384 shaM = new SHA384Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA512")
                {
                    SHA512 shaM = new SHA512Managed();
                    resultAux = shaM.ComputeHash(sh);
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
                byte[] resultAux = null;
                if (hashAlgo == "SHA256" || hashAlgo == "SHA-256")
                {
                    SHA256 shaM = new SHA256Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA1")
                {
                    SHA1 shaM = new SHA1Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA384")
                {
                    SHA384 shaM = new SHA384Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA512")
                {
                    SHA512 shaM = new SHA512Managed();
                    resultAux = shaM.ComputeHash(sh);
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
                byte[] resultAux = null;
                if (hashAlgo == "SHA256" || hashAlgo == "SHA-256")
                {
                    SHA256 shaM = new SHA256Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA1")
                {
                    SHA1 shaM = new SHA1Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA384")
                {
                    SHA384 shaM = new SHA384Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA512")
                {
                    SHA512 shaM = new SHA512Managed();
                    resultAux = shaM.ComputeHash(sh);
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
                byte[] resultAux = null;
                if(hashAlgo == "SHA256" || hashAlgo == "SHA-256")
                {
                    SHA256 shaM = new SHA256Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if(hashAlgo == "SHA1")
                {
                    SHA1 shaM = new SHA1Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA384")
                {
                    SHA384 shaM = new SHA384Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA512")
                {
                    SHA512 shaM = new SHA512Managed();
                    resultAux = shaM.ComputeHash(sh);
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
                byte[] resultAux = null;
                if (hashAlgo == "SHA256" || hashAlgo == "SHA-256")
                {
                    SHA256 shaM = new SHA256Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA1")
                {
                    SHA1 shaM = new SHA1Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA384")
                {
                    SHA384 shaM = new SHA384Managed();
                    resultAux = shaM.ComputeHash(sh);
                }
                else if (hashAlgo == "SHA512")
                {
                    SHA512 shaM = new SHA512Managed();
                    resultAux = shaM.ComputeHash(sh);
                }

                string hashedDocumentB64 = Convert.ToBase64String(resultAux);

                string signParam = null;
                string hashParam = null;
                keysAlgo.TryGetValue(SelectedAlgo, out signParam);
                if (SelectedAlgo == "RSA")
                {
                    keysAlgo.TryGetValue(hashAlgo, out hashParam);
                }

                LoginPage.user.credentialsAuthorize(SelectedKey, hashedDocumentB64, null, null);
                LoginPage.user.signSingleHash(SelectedKey, hashedDocumentB64, signParam, hashParam);
            }

            // ORIGINAL
            sgn.SetExternalDigest(Convert.FromBase64String(LoginPage.user.signatures[0]), null, signAlgo);


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
            

            string outFileName3 = Path.GetDirectoryName(DocPath);
            outFileName3 += @"\";
            outFileName3 += Path.GetFileNameWithoutExtension(DocPath);
            outFileName3 += "SIGNED";
            outFileName3 += Path.GetExtension(DocPath);

            FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

            PdfReader reader3 = new PdfReader(outTempFileName);
            PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

            IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

            PdfSigner.SignDeferred(signer3.GetDocument(), NumeSemnatura, outFile3, signature3);
            outFile3.Dispose();
            outFile3.Close();

            LoginPage.user.signatures.Clear();

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
        private string hashAlgo;

        public PreSignatureContainer(PdfName filter, PdfName subFilter, string algo)
        {
            sigDic = new PdfDictionary();
            sigDic.Put(PdfName.Filter, filter);
            sigDic.Put(PdfName.SubFilter, subFilter);
            hashAlgo = algo;
        }

        public void ModifySigningDictionary(PdfDictionary signDic)
        {
            signDic.PutAll(sigDic);
        }

        public byte[] Sign(Stream data)
        {
            if (hashAlgo == string.Empty)
            {
                this.hash = DigestAlgorithms.Digest(data, DigestAlgorithms.SHA256);

                return new byte[0];
            }

            try
            {
                this.hash = DigestAlgorithms.Digest(data, hashAlgo);
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


    public static class Helper
    {
        public static string GetUntilOrEmpty(this string text, string stopAt = "-")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        public static string GetAfterOrEmpty(this string text, string stopAt = "-")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(charLocation + 1);
                }
            }

            return String.Empty;
        }


    }

}
