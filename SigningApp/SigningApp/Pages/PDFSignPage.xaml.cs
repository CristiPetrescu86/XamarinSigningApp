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
                            { DevicePlatform.UWP, new [] {".pdf",".xml"} }
                        })
                    });


                if (file == null)
                {
                    LabelInfo.Text = "Nu e bun";
                }
                else
                {
                    LabelInfo.Text = file.FullPath;
                    path = file.FullPath;
                }


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


            //List<string> pdfList = new List<string>();
            //pdfList.Add(fileName);
            //pdfList.Add(@"D:\Facultate\Licenta\Github_Lic\SigningApp\XamarinSigningApp\SigningApp\SigningApp\test.pdf");
            //LoginPage.user.credentialsAuthorize(pdfList, LoginPage.user.credentialsIDs[1]);
            //LoginPage.user.signSingleHash(pdfList, LoginPage.user.credentialsIDs[1]);

            //LoginPage.user.signatures[0]

            //label1.Text = LoginPage.user.keysInfo[0].cert.certificates[0];

            //Debug.WriteLine(LoginPage.user.signatures[0]);

            byte[] bytes = Convert.FromBase64String(LoginPage.user.keysInfo[0].cert.certificates[0]);
            var cert = new X509Certificate2(bytes);
            Org.BouncyCastle.X509.X509Certificate cert1 = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);

            byte[] bytes2 = Convert.FromBase64String(LoginPage.user.keysInfo[0].cert.certificates[1]);
            var cert1a = new X509Certificate2(bytes2);
            Org.BouncyCastle.X509.X509Certificate cert1b = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert1a);



            //for(int i=0;i< LoginPage.user.keysInfo[0].cert.certificates.Count;i++)
            //{
            //    string aux1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "certificate");
            //    aux1 += i.ToString() + ".cer";
            //    System.IO.File.WriteAllText(aux1, LoginPage.user.keysInfo[0].cert.certificates[i]);
            //}


            // ---------------------------------------- DEMO

            //string outFileName2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testTEMP.pdf");

            //string fieldname = "sig";

            //PdfReader reader2 = new PdfReader(fileName);
            //PdfSigner signer2 = new PdfSigner(reader2, new FileStream(outFileName2, FileMode.Create), new StampingProperties());
            //PdfSignatureAppearance appearance2 = signer2.GetSignatureAppearance();
            //appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(36, 648, 200, 100));
            //appearance2.SetPageNumber(1);
            //appearance2.SetCertificate(cert1);
            //appearance2.SetReason("For test");
            //appearance2.SetLocation("HKSAR");
            //signer2.SetFieldName(fieldname);

            //IExternalSignatureContainer external = new ExternalBlankSignatureContainer(PdfName.Adobe_PPKLite,PdfName.Adbe_pkcs7_detached);
            //signer2.SignExternalContainer(external, 8192);


            //// trimit pdf temporar
            //List<string> pdfList = new List<string>();
            //pdfList.Add(fileName);
            //LoginPage.user.credentialsAuthorize(pdfList, LoginPage.user.credentialsIDs[1]);
            //LoginPage.user.signSingleHash(pdfList, LoginPage.user.credentialsIDs[1]);

            Org.BouncyCastle.X509.X509Certificate[] cert2 = new Org.BouncyCastle.X509.X509Certificate[2];
            cert2[0] = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);
            cert2[1] = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert1a);






            //byte[] sig = Convert.FromBase64String(LoginPage.user.signatures[0]);
            ////string outFileName43 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "sign.txt");



            //string outFileName3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testTEMPSigned.pdf");
            //FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

            //PdfReader reader3 = new PdfReader(outFileName2);
            ////PdfReader reader = new PdfReader(@"D:\Facultate\Licenta\Github_Lic\SigningApp\XamarinSigningApp\SigningApp\SigningApp\test.pdf");
            //PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties().UseAppendMode());

            //IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(sig);

            //PdfSigner.SignDeferred(signer3.GetDocument(), fieldname, outFile3, signature3);
            //outFile3.Close();

            //string outFileName4 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test2Signed.pdf");
            //FileStream out4 = new FileStream(outFileName4, FileMode.Create);

            //PdfReader reader4 = new PdfReader(fileName);
            //PdfSigner signer4 = new PdfSigner(reader4, out4, new StampingProperties());
            //PdfSignatureAppearance appearance4 = signer4.GetSignatureAppearance();
            //appearance4.SetPageRect(new iText.Kernel.Geom.Rectangle(36, 648, 200, 100));
            //appearance4.SetPageNumber(1);
            //appearance4.SetCertificate(cert1);
            //appearance4.SetReason("For test");
            //appearance4.SetLocation("HKSAR");
            //signer4.SetCertificationLevel(PdfSigner.NOT_CERTIFIED);



            //IExternalSignature signature2 = new MyExternSign(encodedSig);
            //signer4.SignDetached(signature2, cert2, null, null, null, 8192, PdfSigner.CryptoStandard.CADES);
            //out4.Close();





            // ---------------------------------------- END DEMO

            

            // DEMO GOKU ----------------------------------------------------

            string outFileName2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testGOKUtemp.pdf");

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



            string outFileName3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testGokuSIGNED.pdf");

            FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

            PdfReader reader3 = new PdfReader(outFileName2);
            PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

            IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

            PdfSigner.SignDeferred(signer3.GetDocument(), fieldname, outFile3, signature3);
            outFile3.Close();


            // END DEMO GOKU ------------------------------------------------
            






            //string outFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testSigned.pdf");

            //PdfReader reader = new PdfReader(fileName);
            ////PdfReader reader = new PdfReader(@"D:\Facultate\Licenta\Github_Lic\SigningApp\XamarinSigningApp\SigningApp\SigningApp\test.pdf");
            //PdfSigner signer = new PdfSigner(reader, new FileStream(outFileName, FileMode.Create), new StampingProperties().UseAppendMode());



            //// Create the signature appearance
            //iText.Kernel.Geom.Rectangle rect = new iText.Kernel.Geom.Rectangle(36, 648, 200, 100);
            //PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
            //appearance.SetContact("ContactInfo");
            //appearance.SetLocation("Location");
            //appearance.SetPageNumber(1);
            //appearance.SetReason("Reason");
            //appearance.SetPageRect(rect);
            //appearance.SetCertificate(cert1);

            //IExternalSignatureContainer signature = new ExternalBlankSignatureContainer(PdfName.Adobe_PPKLite, PdfName.Adbe_pkcs7_detached);

            //signer.SignExternalContainer(signature, 8192);

            //string outFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "testSigned.pdf");
            //FileStream file = new FileStream(outFileName, FileMode.Create, FileAccess.Write);

            //byte[] bytes2 = new byte[newStream.Length];
            //newStream.Read(bytes2, 0, (int)newStream.Length);
            //file.Write(bytes2, 0, bytes2.Length);
            //newStream.Close();

            //newStream.Write(outFileName);


            // Sign the document using the detached mode, CMS or CAdES equivalent.
            //signer.SignDetached(hashedDocumentB64 , signature, cert1, null, null, null,
            //        0, null);
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