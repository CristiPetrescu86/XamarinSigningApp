using SigningApp.XadesSignedXML.XML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinLicentaApp;

namespace SigningApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class XMLSignPage : ContentPage
    {
        public XMLSignPage()
        {
            InitializeComponent();
        }


        private async void filePickerButton(object sender, EventArgs e)
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

        private void btnSignClicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "xmlFile1.xml");

            LoginPage.user.credentialsList();
            LoginPage.user.credentialsInfo(LoginPage.user.credentialsIDs[1]);


            //byte[] bytes = Convert.FromBase64String(LoginPage.user.keysInfo[0].cert.certificates[0]);
            //var cert = new X509Certificate2(bytes);
            //Org.BouncyCastle.X509.X509Certificate cert1 = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert);

            /*
            XAdES signature = new XAdES(cert);
            String output = signature.Sign(bytes2, true);


            string filePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "xmlFile5Signed.xml");
            FileStream fsOut = new FileStream(filePath2, FileMode.Create, FileAccess.Write);
            fsOut.Write(Encoding.ASCII.GetBytes(output), 0, output.Length);
            fsOut.Close();
            */

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //Console.WriteLine(doc.OuterXml);

            //string filePFX = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test-cert.pfx");
            //X509Certificate2 cert2 = new X509Certificate2(File.ReadAllBytes(filePFX), "cristi");

            //byte[] bytes = Convert.FromBase64String(LoginPage.user.keysInfo[0].cert.certificates[0]);
            //var certOK = new X509Certificate2(bytes);

            List<X509Certificate2> certList = new List<X509Certificate2>();
            foreach (string elem in LoginPage.user.keysInfo[0].cert.certificates)
            {
                byte[] bytes = Convert.FromBase64String(elem);
                certList.Add(new X509Certificate2(bytes));
            }

            if(certList == null)
            {
                throw new Exception();
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

            
            XadesObject xo = new XadesObject();
            {
                Cert cert2 = new Cert();

                cert2.IssuerSerial.X509IssuerName = certList[0].IssuerName.Name;
                cert2.IssuerSerial.X509SerialNumber = certList[0].SerialNumber;

                {
                    SHA256 cryptoServiceProvider = new SHA256CryptoServiceProvider();
                    cert2.CertDigest.DigestValue = cryptoServiceProvider.ComputeHash(certList[0].RawData);
                    cert2.CertDigest.DigestMethod.Algorithm = SignedXml.XmlDsigSHA256Url;
                }

                Cert cert3 = new Cert();

                cert3.IssuerSerial.X509IssuerName = certList[1].IssuerName.Name;
                cert3.IssuerSerial.X509SerialNumber = certList[1].SerialNumber;

                {
                    SHA256 cryptoServiceProvider = new SHA256CryptoServiceProvider();
                    cert3.CertDigest.DigestValue = cryptoServiceProvider.ComputeHash(certList[1].RawData);
                    cert3.CertDigest.DigestMethod.Algorithm = SignedXml.XmlDsigSHA256Url;
                }



                xo.QualifyingProperties.Target = "#" + signedXml.Signature.Id;
                xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningTime = DateTime.Now;
                xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyImplied = true;

                xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningCertificate.CertCollection.Add(cert2);
                xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningCertificate.CertCollection.Add(cert3);
                

                //DataObjectFormat dof = new DataObjectFormat();
                //dof.ObjectReferenceAttribute = "#Document";
                //dof.Description = "Document xml[XML]";
                //dof.Encoding = SignedXml.XmlDsigBase64TransformUrl;
                //dof.MimeType = "text/plain";
                //xo.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection.Add(dof);
            }
            signedXml.AddXadesObject(xo);
            
            byte[] toBeSigned = signedXml.ComputeSignature();

            LoginPage.user.credentialsAuthorize(LoginPage.user.credentialsIDs[1], toBeSigned, "XML", "12345678", "123456");
            LoginPage.user.signSingleHash(LoginPage.user.credentialsIDs[1], toBeSigned, "XML");

            signedXml.setSignatureValue(LoginPage.user.signatures[0]);

            XmlElement xmlSig = signedXml.GetXml();

            doc.DocumentElement.AppendChild(doc.ImportNode(xmlSig, true));

            //Console.WriteLine(xmlSig.OuterXml);
            //Console.WriteLine(Convert.ToBase64String(signedXml.Signature.SignatureValue));

            string filePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SignedNow1.xml");
            //doc.Save(filePath2);
            File.WriteAllText(filePath2, doc.OuterXml);



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