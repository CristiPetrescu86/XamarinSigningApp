using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace SignatureXML.Library
{
    public class XMLSignature
    {
        private SignedXml signedXml;
        private XmlDocument doc;
        private string docPath;

        public byte[] getSignatureContent(ConfigureClass configureClass)
        {
            docPath = configureClass.fileName;

            // Initializam documentul XML
            doc = new XmlDocument();
            doc.Load(configureClass.fileName);

            // Cream tot lantul de certificate
            List<X509Certificate2> certList = new List<X509Certificate2>();
            foreach (string elem in configureClass.keyObject.cert.certificates)
            {
                byte[] bytes = Convert.FromBase64String(elem);
                certList.Add(new X509Certificate2(bytes));
            }

            if (certList == null)
            {
                return null;
            }

            // Creare id random
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = "xmldsig-" + myuuid.ToString();

            // Creare obiect SignedXml din clasa modificata de noi
            signedXml = new SignedXml(doc);
            // Atasare cheie publica
            signedXml.PublicKeyCert = certList[0].PublicKey.Key;
            // Nume Random
            signedXml.Signature.Id = myuuidAsString;
            // Defineste referinta in care se afla rezumatul documentului si algoritmul cu care a fost facut
            Reference reference = new Reference();
            if (configureClass.hashAlgo == "SHA256" || configureClass.hashAlgo == "SHA-256")
                reference.DigestMethod = SignedXml.XmlDsigSHA256Url;
            else if (configureClass.hashAlgo == "SHA1")
                reference.DigestMethod = SignedXml.XmlDsigSHA1Url;
            else if (configureClass.hashAlgo == "SHA384")
                reference.DigestMethod = SignedXml.XmlDsigSHA384Url;
            else if (configureClass.hashAlgo == "SHA512")
                reference.DigestMethod = SignedXml.XmlDsigSHA512Url;

            reference.Uri = "";
            // Executa transformarea referintei
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);
            // Adauga referinta in semnatura
            signedXml.AddReference(reference);

            // Adauga campul KeyInfo in care se afla lantul de certificate al semnatarului
            KeyInfo keyInfo = new KeyInfo();
            KeyInfoX509Data dataCerts = new KeyInfoX509Data();
            foreach (X509Certificate2 elem in certList)
            {
                dataCerts.AddCertificate(elem);
            }
            keyInfo.AddClause(dataCerts);
            signedXml.KeyInfo = keyInfo;

            if (configureClass.selectedType)
            {
                // Creare obiect XAdES
                XadesObject xo = new XadesObject();
                {
                    List<Cert> certAuxList = new List<Cert>();

                    // Se creaza campul Cert, in care sunt hash-urile certificatelor
                    foreach (X509Certificate2 elem in certList)
                    {
                        Cert certAux = new Cert();

                        // Adaugare IssuerName
                        certAux.IssuerSerial.X509IssuerName = elem.IssuerName.Name;
                        // Adaugare SerialNumber
                        certAux.IssuerSerial.X509SerialNumber = elem.SerialNumber;
                        {
                            SHA256 cryptoServiceProvider = new SHA256CryptoServiceProvider();
                            certAux.CertDigest.DigestValue = cryptoServiceProvider.ComputeHash(elem.RawData);
                            certAux.CertDigest.DigestMethod.Algorithm = SignedXml.XmlDsigSHA256Url;
                        }

                        certAuxList.Add(certAux);
                    }

                    // Se adauga id-ul obiectului si data si timpul semnarii
                    xo.QualifyingProperties.Target = "#" + signedXml.Signature.Id;
                    xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningTime = DateTime.Now;
                    xo.QualifyingProperties.SignedProperties.SignedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyImplied = true;

                    // Se adauga in SignedProperties a campului Cert
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
                signedXml.AddXadesObject(xo); // Se adauga obiectul XAdES
            }

            // Se adauga Url-ul algoritmului de semnare
            if (configureClass.selectedAlgo == "RSA-SHA1")
                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
            else if (configureClass.selectedAlgo == "RSA-SHA256")
                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
            else if (configureClass.selectedAlgo == "RSA-SHA384")
                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA384Url;
            else if (configureClass.selectedAlgo == "RSA-SHA512")
                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA512Url;
            else if (configureClass.signAlgo == "RSA" && configureClass.hashAlgo == "SHA-256")
                signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            // Se extrage valoarea care trebuie semnata cu cheia privata
            return signedXml.ComputeSignature();
        }

        public bool attachSignatureToDoc(string signatureValue)
        {
            // Se adauga semnatura valida
            signedXml.setSignatureValue(signatureValue);

            // Se adauga semnatura in documentul initial
            XmlElement xmlSig = signedXml.GetXml();
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlSig, true));

            string outFileName = Path.GetDirectoryName(docPath);
            outFileName += @"\";
            outFileName += Path.GetFileNameWithoutExtension(docPath);
            outFileName += "SIGNED";
            outFileName += Path.GetExtension(docPath);

            //doc.Save(outFileName);
            File.WriteAllText(outFileName, doc.OuterXml);

            return true;
        }



    }
}
