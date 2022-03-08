using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            

        }


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
                xmlDocument.LoadXml(xml);

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
    }
}