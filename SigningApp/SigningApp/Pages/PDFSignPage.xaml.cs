using iText.Kernel.Pdf;
using iText.Signatures;
using LicentaApp;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;


namespace XamarinLicentaApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PDFSignPage : ContentPage
    {
        public PDFSignPage()
        {
            InitializeComponent();
        }

        private string path;

        private async void ButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // pickMultipleAsync pentru mai tarziu
                var file = await FilePicker.PickAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                        {
                            { DevicePlatform.UWP, new [] {".pdf",".xml"} },
                            { DevicePlatform.Android, new [] {".pdf",".xml"} },
                            { DevicePlatform.macOS, new [] {".pdf",".xml"} },
                            { DevicePlatform.iOS, new [] {".pdf",".xml"} }
                        })
                    });




            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ButtonClicked2(object sender, EventArgs e)
        {
            LoginPage.user.credentialsList();
            LoginPage.user.credentialsInfo(LoginPage.user.credentialsIDs[1]);


            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.pdf");


            byte[] bytes = Convert.FromBase64String(LoginPage.user.keysInfo[0].cert.certificates[0]);
            var cert = new X509Certificate2(bytes);
            Org.BouncyCastle.X509.X509Certificate cert1 = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);

            byte[] bytes2 = Convert.FromBase64String(LoginPage.user.keysInfo[0].cert.certificates[1]);
            var cert1a = new X509Certificate2(bytes2);
            Org.BouncyCastle.X509.X509Certificate cert1b = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert1a);
          

            Org.BouncyCastle.X509.X509Certificate[] cert2 = new Org.BouncyCastle.X509.X509Certificate[2];
            cert2[0] = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);
            cert2[1] = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert1a);

            

            // Signature ----------------------------------------------------

            string outFileName2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testTemp.pdf");

            string fieldname = "sig";

            PdfReader reader2 = new PdfReader(fileName);
            PdfSigner signer2 = new PdfSigner(reader2, new FileStream(outFileName2,FileMode.Create), new StampingProperties());

            PdfSignatureAppearance appearance2 = signer2.GetSignatureAppearance();
            appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(36, 648, 200, 100));
            appearance2.SetPageNumber(1);
            appearance2.SetCertificate(cert1);
            appearance2.SetReason("For test");
            appearance2.SetLocation("HKSAR");
            appearance2.SetSignatureCreator("iTextSharp7");
            //signer2.SetCertificationLevel(PdfSigner.CERTIFIED_NO_CHANGES_ALLOWED);
            signer2.SetFieldName(fieldname);

            PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED);
            signer2.SignExternalContainer(external, 16000);
            byte[] documentHash = external.getHash();



            PdfPKCS7 sgn = new PdfPKCS7(null, cert2, "SHA256", false);
            byte[] sh = sgn.GetAuthenticatedAttributeBytes(documentHash, PdfSigner.CryptoStandard.CADES, null, null);


            List<string> aux = new List<string>();
            aux.Add("salut");
            LoginPage.user.credentialsAuthorize(aux, LoginPage.user.credentialsIDs[1],sh);
            LoginPage.user.signSingleHash(aux, LoginPage.user.credentialsIDs[1],sh);


            sgn.SetExternalDigest(Convert.FromBase64String(LoginPage.user.signatures[0]), null, "RSA");
            byte[] encodedSig = sgn.GetEncodedPKCS7(documentHash, PdfSigner.CryptoStandard.CADES, null, null, null);



            string outFileName3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testSigned.pdf");

            FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

            PdfReader reader3 = new PdfReader(outFileName2);
            PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

            IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

            PdfSigner.SignDeferred(signer3.GetDocument(), fieldname, outFile3, signature3);
            outFile3.Close();


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

    public class MyExternSign : IExternalSignature
    {
        protected byte[] sig;

        public MyExternSign(byte[] signature)
        {
            this.sig = signature;
        }

        public string GetEncryptionAlgorithm()
        {
            return "RSA";
        }

        public string GetHashAlgorithm()
        {
            return "SHA-256";
        }

        public byte[] Sign(byte[] message)
        {
            return this.sig;
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