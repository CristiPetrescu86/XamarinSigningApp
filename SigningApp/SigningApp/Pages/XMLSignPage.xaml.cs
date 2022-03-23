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
            

            signedXml.ComputeSignature();
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


        class XAdES
        {
            protected X509Certificate2 x509Certificate2;
            protected RSACryptoServiceProvider csp;

            public XAdES(X509Certificate2 x509Certificate2)
            {
                this.x509Certificate2 = x509Certificate2;
                this.csp = (RSACryptoServiceProvider)x509Certificate2.PrivateKey;
            }

            public String Sign(String sFolder, String sFileName, bool boEnveloped)
            {
                byte[] fileXML = File.ReadAllBytes(sFolder + "\\" + sFileName);

                return Sign(fileXML, boEnveloped);
            }

            public String Sign(String sXMLDocument, bool boEnveloped)
            {
                byte[] fileXML = System.Text.Encoding.Default.GetBytes(sXMLDocument);

                return Sign(fileXML, boEnveloped);
            }

            public byte[] C14NTransform(byte[] fileXML)
            {
                Stream stream = new MemoryStream(fileXML);

                XmlDsigC14NTransform xmlDsigC14NTransform = new XmlDsigC14NTransform(true);
                xmlDsigC14NTransform.LoadInput(stream);

                Type streamType = typeof(System.IO.Stream);
                MemoryStream outputStream = (MemoryStream)xmlDsigC14NTransform.GetOutput(streamType);

                return outputStream.ToArray();
            }

            public String Sign(byte[] fileXML, bool boEnveloped)
            {
                byte[] fileC14NXML = C14NTransform(fileXML);

                // Hash the data
                SHA256Managed sha256 = new SHA256Managed();
                byte[] hashFileXml = sha256.ComputeHash(fileC14NXML);

                String sB64_Hash = System.Convert.ToBase64String(hashFileXml, Base64FormattingOptions.None);
                String sB64_Cert = System.Convert.ToBase64String(x509Certificate2.GetRawCertData(), Base64FormattingOptions.None);

                String sKeyInfo = "<KeyInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\" Id=\"idKeyInfo\">";
                sKeyInfo += "<X509Data>";
                sKeyInfo += "<X509Certificate>" + sB64_Cert + "</X509Certificate>";
                sKeyInfo += "</X509Data>";
                sKeyInfo += "</KeyInfo>";
                byte[] abKeyInfo = System.Text.Encoding.Default.GetBytes(sKeyInfo);
                byte[] hashKeyInfo = sha256.ComputeHash(abKeyInfo);
                String sB64_HKey = System.Convert.ToBase64String(hashKeyInfo, Base64FormattingOptions.None);

                String sTimestamp = GetTimestamp();
                String sSignedProperties = "<SignedProperties xmlns=\"http://uri.etsi.org/01903/v1.3.2#\" Id=\"idSignedProperties\">";
                sSignedProperties += "<SignedSignatureProperties>";
                sSignedProperties += "<SigningTime>" + sTimestamp + "</SigningTime>";
                // byte[] certContent = x509Certificate2.GetRawCertData();
                byte[] certContent = System.Text.Encoding.Default.GetBytes(sB64_Cert);
                byte[] certHash = sha256.ComputeHash(certContent);
                String certHashUse = Convert.ToBase64String(certHash);
                String certIssuerName = x509Certificate2.Issuer.ToString();
                String certSerianNumber = x509Certificate2.SerialNumber.ToString();
                sSignedProperties += "<SigningCertificate>";
                sSignedProperties += "<Cert>";
                sSignedProperties += "<CertDigest><DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\"/>";
                sSignedProperties += "<DigestValue>" + certHashUse + "</DigestValue></CertDigest>";
                sSignedProperties += "<IssuerSerial>";
                sSignedProperties += "<X509IssuerName>" + certIssuerName + "</X509IssuerName>";
                sSignedProperties += "<X509SerialNumber>" + certSerianNumber + "</X509SerialNumber>";
                sSignedProperties += "</IssuerSerial>";
                sSignedProperties += "</Cert>";
                sSignedProperties += "</SigningCertificate>";
                sSignedProperties += "</SignedSignatureProperties>";
                sSignedProperties += "</SignedProperties>";
                byte[] abSignedProperties = System.Text.Encoding.Default.GetBytes(sSignedProperties);
                byte[] hashSignedProperties = sha256.ComputeHash(abSignedProperties);
                String sB64_HSPr = System.Convert.ToBase64String(hashSignedProperties, Base64FormattingOptions.None);

                String sIdRef_1 = "xmldsig-" + Guid.NewGuid().ToString("D");
                String sRef_1 = "<Reference Id=\"" + sIdRef_1 + "\" URI=\"\">";
                sRef_1 += "<Transforms>";
                sRef_1 += "<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"></Transform>";
                sRef_1 += "<Transform Algorithm=\"http://www.w3.org/2006/12/xml-c14n11#WithComments\"></Transform>";
                sRef_1 += "</Transforms>";
                sRef_1 += "<DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\"></DigestMethod>";
                sRef_1 += "<DigestValue>" + sB64_Hash + "</DigestValue>";
                sRef_1 += "</Reference>";

                String sIdRef_2 = "xmldsig-" + Guid.NewGuid().ToString("D");
                String sRef_2 = "<Reference Id=\"" + sIdRef_2 + "\" URI=\"#idKeyInfo\">";
                sRef_2 += "<DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\"></DigestMethod>";
                sRef_2 += "<DigestValue>" + sB64_HKey + "</DigestValue>";
                sRef_2 += "</Reference>";

                String sIdRef_3 = "xmldsig-" + Guid.NewGuid().ToString("D");
                String sRef_3 = "<Reference Id=\"" + sIdRef_3 + "\" Type=\"http://uri.etsi.org/01903#SignedProperties\" URI=\"#idSignedProperties\">";
                sRef_3 += "<DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\"></DigestMethod>";
                sRef_3 += "<DigestValue>" + sB64_HSPr + "</DigestValue>";
                sRef_3 += "</Reference>";

                String sSignedInfo = "<SignedInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\">";
                sSignedInfo += "<CanonicalizationMethod Algorithm=\"http://www.w3.org/2006/12/xml-c14n11#WithComments\"></CanonicalizationMethod>";
                sSignedInfo += "<SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\"></SignatureMethod>";
                sSignedInfo += sRef_1;
                sSignedInfo += sRef_2;
                sSignedInfo += sRef_3;
                sSignedInfo += "</SignedInfo>";
                byte[] abSignedInfo = System.Text.Encoding.Default.GetBytes(sSignedInfo);
                byte[] hashSignedInfo = sha256.ComputeHash(abSignedInfo);

                // Sign the hash of sSignedInfo
                //RSAPKCS1SignatureFormatter rsaPKCS1SignatureFormatter = new RSAPKCS1SignatureFormatter(csp);
                //rsaPKCS1SignatureFormatter.SetHashAlgorithm("SHA256");
                //byte[] signature = rsaPKCS1SignatureFormatter.CreateSignature(hashSignedInfo);

                LoginPage.user.credentialsAuthorize(null, LoginPage.user.credentialsIDs[1], abSignedInfo);
                LoginPage.user.signSingleHash(null, LoginPage.user.credentialsIDs[1], abSignedInfo);

                //byte[] signature = LoginPage.user.signatures[0];

                //String sB64_Sign = System.Convert.ToBase64String(signature, Base64FormattingOptions.None);

                String sB64_Sign = LoginPage.user.signatures[0];

                String sIdSignature = "xmldsig-" + Guid.NewGuid().ToString("D");
                String sXMLSignature = "<Signature Id=\"" + sIdSignature + "\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\">";
                sXMLSignature += "<SignedInfo>";
                sXMLSignature += "<CanonicalizationMethod Algorithm=\"http://www.w3.org/2006/12/xml-c14n11#WithComments\"></CanonicalizationMethod>";
                sXMLSignature += "<SignatureMethod Algorithm=\"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256\"></SignatureMethod>";
                sXMLSignature += sRef_1;
                sXMLSignature += sRef_2;
                sXMLSignature += sRef_3;
                sXMLSignature += "</SignedInfo>";
                sXMLSignature += "<SignatureValue>" + sB64_Sign + "</SignatureValue>";
                sXMLSignature += "<KeyInfo Id=\"idKeyInfo\">";
                sXMLSignature += "<X509Data>";
                sXMLSignature += "<X509Certificate>" + sB64_Cert + "</X509Certificate>";
                sXMLSignature += "</X509Data>";
                sXMLSignature += "</KeyInfo>";
                sXMLSignature += "<Object Id=\"idObject\">";
                sXMLSignature += "<QualifyingProperties Target=\"#" + sIdSignature + "\" xmlns=\"http://uri.etsi.org/01903/v1.3.2#\">";
                sXMLSignature += "<SignedProperties xmlns=\"http://uri.etsi.org/01903/v1.3.2#\" Id=\"idSignedProperties\">";
                sXMLSignature += "<SignedSignatureProperties>";
                sXMLSignature += "<SigningTime>" + sTimestamp + "</SigningTime>";
                // new --
                //byte[] certContent = x509Certificate2.GetRawCertData();
                //byte[] certHash = sha256.ComputeHash(certContent);
                //String certHashUse = Convert.ToBase64String(certHash);
                //String certIssuerName = x509Certificate2.Issuer.ToString();
                //String certSerianNumber = x509Certificate2.SerialNumber.ToString();
                sXMLSignature += "<SigningCertificate>";
                sXMLSignature += "<Cert>";
                sXMLSignature += "<CertDigest><DigestMethod Algorithm=\"http://www.w3.org/2001/04/xmlenc#sha256\"/>";
                sXMLSignature += "<DigestValue>" + certHashUse + "</DigestValue></CertDigest>";
                sXMLSignature += "<IssuerSerial>";
                sXMLSignature += "<X509IssuerName>" + certIssuerName + "</X509IssuerName>";
                sXMLSignature += "<X509SerialNumber>" + certSerianNumber + "</X509SerialNumber>";
                sXMLSignature += "</IssuerSerial>";
                sXMLSignature += "</Cert>";
                sXMLSignature += "</SigningCertificate>";
                // new --
                sXMLSignature += "</SignedSignatureProperties>";
                sXMLSignature += "</SignedProperties>";
                sXMLSignature += "</QualifyingProperties>";
                sXMLSignature += "</Object>";
                sXMLSignature += "</Signature>";

                if (!boEnveloped)
                {
                    return sXMLSignature;
                }

                String sOutput = System.Text.Encoding.Default.GetString(fileXML);
                String sSingatureCodeTag = "</signatureCode>";
                int iSignatureCode = sOutput.IndexOf(sSingatureCodeTag);
                if (iSignatureCode > 0)
                {
                    int iStartSignature = iSignatureCode + sSingatureCodeTag.Length;
                    return sOutput.Substring(0, iStartSignature) + sXMLSignature + sOutput.Substring(iStartSignature);
                }
                int iLastTag = sOutput.LastIndexOf("</");
                return sOutput.Substring(0, iLastTag) + sXMLSignature + sOutput.Substring(iLastTag);
            }

            private String GetTimestamp()
            {
                DateTime dtCurrent = DateTime.Now.ToUniversalTime();
                String sYear = "" + dtCurrent.Year;
                String sMonth = dtCurrent.Month < 10 ? "0" + dtCurrent.Month : "" + dtCurrent.Month;
                String sDay = dtCurrent.Day < 10 ? "0" + dtCurrent.Day : "" + dtCurrent.Day;
                String sHour = dtCurrent.Hour < 10 ? "0" + dtCurrent.Hour : "" + dtCurrent.Hour;
                String sMinute = dtCurrent.Minute < 10 ? "0" + dtCurrent.Minute : "" + dtCurrent.Minute;
                String sSecond = dtCurrent.Second < 10 ? "0" + dtCurrent.Second : "" + dtCurrent.Second;
                return sYear + "-" + sMonth + "-" + sDay + "T" + sHour + ":" + sMinute + ":" + sSecond + "Z";
            }
        }



        /*

        public static class Signature
        {
            #region Private fields
            public const string XmlDsigSignatureProperties = "http://uri.etsi.org/01903#SignedProperties";
            public const string XadesProofOfApproval = "http://uri.etsi.org/01903/v1.2.2#ProofOfApproval";
            public const string XadesPrefix = "xades";
            public const string SignaturePrefix = "ds";
            public const string SignatureNamespace = "http://www.w3.org/2000/09/xmldsig#";
            public const string XadesNamespaceUrl = "http://uri.etsi.org/01903/v1.3.2#";

            private const string SignatureId = "Signature";
            private const string SignaturePropertiesId = "SignedProperties";
            #endregion Private fields

            #region Public methods
            public static XmlElement SignWithXades(X509Certificate2 signingCertificate, string xml)
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(xml);

                AddSignatureProperties(xmlDocument, signingCertificate);

                var signedXml = new SignedXml(xmlDocument);
                signedXml.Signature.Id = SignatureId;
                signedXml.SigningKey = signingCertificate.PrivateKey;


                var signatureReference = new Reference { Uri = "", };
                signatureReference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                signedXml.AddReference(signatureReference);

                var parametersSignature = new Reference
                {
                    Uri = $"#{SignaturePropertiesId}",
                    Type = XmlDsigSignatureProperties,
                };
                signedXml.AddReference(parametersSignature);

                var keyInfo = new KeyInfo();
                keyInfo.AddClause(new KeyInfoX509Data(signingCertificate));
                signedXml.KeyInfo = keyInfo;

                //LoginPage.user.signSingleHash(null, LoginPage.user.credentialsIDs[1], )
                signedXml.ComputeSignature();

                var signatureNode = signedXml.GetXml();

                AssignNameSpacePrefixToElementTree(signatureNode, SignaturePrefix);

                var signedInfoNode = signatureNode.SelectSingleNode("//*[local-name()='SignedInfo']");
                var signatureValue = signatureNode.SelectSingleNode("//*[local-name()='SignatureValue']");
                var keyInfoNode = xmlDocument.SelectSingleNode("//*[local-name()='KeyInfo']");

                var finalSignatureNode = xmlDocument.SelectSingleNode("//*[local-name()='Signature']");
                finalSignatureNode.InsertBefore(signatureValue, keyInfoNode);
                finalSignatureNode.InsertBefore(signedInfoNode, signatureValue);

                return (XmlElement)finalSignatureNode;
            }
            #endregion Public methods

            #region Private methods
            private static void AddSignatureProperties(XmlDocument document, X509Certificate2 signingCertificate)
            {
                // <Signature>
                var signatureNode = document.CreateElement(SignaturePrefix, "Signature", SignatureNamespace);
                var signatureIdAttribute = document.CreateAttribute("Id");
                signatureIdAttribute.InnerText = SignatureId;
                signatureNode.Attributes.Append(signatureIdAttribute);
                document.DocumentElement.AppendChild(signatureNode);

                AddKeyInfo(document, signatureNode, signingCertificate);
                AddCertificateObject(document, signatureNode, signingCertificate);
            }

            private static void AddKeyInfo(XmlDocument document, XmlElement signatureNode, X509Certificate2 signingCertificate)
            {
                // <KeyInfo>
                var keyInfoNode = document.CreateElement(SignaturePrefix, "KeyInfo", SignatureNamespace);
                signatureNode.AppendChild(keyInfoNode);

                // <KeyInfo><X509Data>
                var x509DataNode = document.CreateElement(SignaturePrefix, "X509Data", SignatureNamespace);
                keyInfoNode.AppendChild(x509DataNode);

                var x509CertificateNode = document.CreateElement(SignaturePrefix, "X509Certificate", SignatureNamespace);
                x509CertificateNode.InnerText = Convert.ToBase64String(signingCertificate.GetRawCertData());
                x509DataNode.AppendChild(x509CertificateNode);
            }

            private static void AddCertificateObject(XmlDocument document, XmlElement signatureNode, X509Certificate2 signingCertificate)
            {
                // <Object>
                var objectNode = document.CreateElement(SignaturePrefix, "Object", SignatureNamespace);
                signatureNode.AppendChild(objectNode);

                // <Object><QualifyingProperties>
                var qualifyingPropertiesNode = document.CreateElement(XadesPrefix, "QualifyingProperties", XadesNamespaceUrl);
                qualifyingPropertiesNode.SetAttribute("Target", $"#{SignatureId}");
                objectNode.AppendChild(qualifyingPropertiesNode);

                // <Object><QualifyingProperties><SignedProperties>
                var signedPropertiesNode = document.CreateElement(XadesPrefix, "SignedProperties", XadesNamespaceUrl);
                signedPropertiesNode.SetAttribute("Id", SignaturePropertiesId);
                qualifyingPropertiesNode.AppendChild(signedPropertiesNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties>
                var signedSignaturePropertiesNode = document.CreateElement(XadesPrefix, "SignedSignatureProperties", XadesNamespaceUrl);
                signedPropertiesNode.AppendChild(signedSignaturePropertiesNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties> </SigningTime>
                var signingTime = document.CreateElement(XadesPrefix, "SigningTime", XadesNamespaceUrl);
                signingTime.InnerText = $"{DateTime.UtcNow.ToString("s")}Z";
                signedSignaturePropertiesNode.AppendChild(signingTime);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate>
                var signingCertificateNode = document.CreateElement(XadesPrefix, "SigningCertificate", XadesNamespaceUrl);
                signedSignaturePropertiesNode.AppendChild(signingCertificateNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert>
                var certNode = document.CreateElement(XadesPrefix, "Cert", XadesNamespaceUrl);
                signingCertificateNode.AppendChild(certNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><CertDigest>
                var certDigestNode = document.CreateElement(XadesPrefix, "CertDigest", XadesNamespaceUrl);
                certNode.AppendChild(certDigestNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><CertDigest> </DigestMethod>
                var digestMethod = document.CreateElement("ds", "DigestMethod", SignedXml.XmlDsigNamespaceUrl);
                var digestMethodAlgorithmAtribute = document.CreateAttribute("Algorithm");
                digestMethodAlgorithmAtribute.InnerText = SignedXml.XmlDsigSHA1Url;
                digestMethod.Attributes.Append(digestMethodAlgorithmAtribute);
                certDigestNode.AppendChild(digestMethod);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><CertDigest> </DigestMethod>
                var digestValue = document.CreateElement("ds", "DigestValue", SignedXml.XmlDsigNamespaceUrl);
                digestValue.InnerText = Convert.ToBase64String(signingCertificate.GetCertHash());
                certDigestNode.AppendChild(digestValue);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><IssuerSerial>
                var issuerSerialNode = document.CreateElement(XadesPrefix, "IssuerSerial", XadesNamespaceUrl);
                certNode.AppendChild(issuerSerialNode);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><IssuerSerial> </X509IssuerName>
                var x509IssuerName = document.CreateElement("ds", "X509IssuerName", SignedXml.XmlDsigNamespaceUrl);
                x509IssuerName.InnerText = signingCertificate.Issuer;
                issuerSerialNode.AppendChild(x509IssuerName);

                // <Object><QualifyingProperties><SignedProperties><SignedSignatureProperties><SigningCertificate><Cert><IssuerSerial> </X509SerialNumber>
                var x509SerialNumber = document.CreateElement("ds", "X509SerialNumber", SignedXml.XmlDsigNamespaceUrl);
                x509SerialNumber.InnerText = ToDecimalString(signingCertificate.SerialNumber);
                issuerSerialNode.AppendChild(x509SerialNumber);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties>
                var signedDataObjectPropertiesNode = document.CreateElement(XadesPrefix, "SignedDataObjectProperties", XadesNamespaceUrl);
                signedPropertiesNode.AppendChild(signedDataObjectPropertiesNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication>
                var commitmentTypeIndicationNode = document.CreateElement(XadesPrefix, "CommitmentTypeIndication", XadesNamespaceUrl);
                signedDataObjectPropertiesNode.AppendChild(commitmentTypeIndicationNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication><CommitmentTypeId>
                var commitmentTypeIdNode = document.CreateElement(XadesPrefix, "CommitmentTypeId", XadesNamespaceUrl);
                commitmentTypeIndicationNode.AppendChild(commitmentTypeIdNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication><CommitmentTypeId><Identifier>
                var identifierNode = document.CreateElement(XadesPrefix, "Identifier", XadesNamespaceUrl);
                identifierNode.InnerText = XadesProofOfApproval;
                commitmentTypeIdNode.AppendChild(identifierNode);

                // <Object><QualifyingProperties><SignedProperties><SignedDataObjectProperties><CommitmentTypeIndication><AllSignedDataObjects>
                var allSignedDataObjectsNode = document.CreateElement(XadesPrefix, "AllSignedDataObjects", XadesNamespaceUrl);
                commitmentTypeIndicationNode.AppendChild(allSignedDataObjectsNode);
            }

            private static string ToDecimalString(string serialNumber)
            {
                BigInteger bi;

                if (BigInteger.TryParse(serialNumber, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bi))
                {
                    return bi.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return serialNumber;
                }
            }

            private static void AssignNameSpacePrefixToElementTree(XmlElement element, string prefix)
            {
                element.Prefix = prefix;

                foreach (var child in element.ChildNodes)
                {
                    if (child is XmlElement)
                        AssignNameSpacePrefixToElementTree(child as XmlElement, prefix);
                }
            }
            #endregion Private methods
        }
        */




    }
}