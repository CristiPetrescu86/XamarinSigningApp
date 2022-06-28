using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SignaturePDF.Library
{
    public class PDFSignature
    {
        private string tempFileName;
        private PdfPKCS7 sgn;
        private byte[] documentHash;
        private string docPathAux;
        private string fieldName;

        public byte[] createSign(ParametersClass panelParams)
        {
            List<X509Certificate2> certList = new List<X509Certificate2>();
            foreach (string elem in panelParams.keyObject.cert.certificates)
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

            docPathAux = panelParams.docPath;
            string outTempFileName = Path.GetDirectoryName(panelParams.docPath);
            Guid myuuid = Guid.NewGuid();
            outTempFileName += @"\";
            outTempFileName += myuuid.ToString();
            outTempFileName += ".TEMP";

            fieldName = panelParams.fieldName;

            tempFileName = outTempFileName;

            using (FileStream outFile2 = new FileStream(outTempFileName, FileMode.Create))
            {
                PdfReader reader = new PdfReader(panelParams.docPath);
                PdfSigner signer = new PdfSigner(reader, outFile2, new StampingProperties().UseAppendMode());

                // Definire aspect chenar
                PdfSignatureAppearance appearance2 = signer.GetSignatureAppearance();
                // Dimensiuni chenar
                appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(panelParams.castedXCoord, panelParams.castedYCoord, panelParams.castedWidthDist, panelParams.castedHeightDist));
                // Pagina din cadrul documnetului pe care sa se afle
                if (panelParams.selectedPage == 0)
                    appearance2.SetPageNumber(1);
                else
                    appearance2.SetPageNumber(panelParams.selectedPage);
                // Certificatul digital al semnatarului
                appearance2.SetCertificate(certListX509[0]);
                // Motiv
                if (panelParams.motiv != null)
                    appearance2.SetReason(panelParams.motiv);
                // Locatie
                if (panelParams.locatie != null)
                    appearance2.SetLocation(panelParams.locatie);
                appearance2.SetSignatureCreator("iTextSharp7 with Bounty Castle");

                // Signature Pad
                if (panelParams.imageData != null)
                {
                    ImageData image1 = ImageDataFactory.Create(panelParams.imageData);
                    appearance2.SetSignatureGraphic(image1);
                    appearance2.SetImageScale(1);
                    appearance2.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);
                }

                // Numele chenarului, unic in cadrul documentului
                signer.SetFieldName(panelParams.fieldName);

                PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED, panelParams.hashAlgo);
                signer.SignExternalContainer(external, 16000);
                documentHash = external.getHash();

                outFile2.Dispose();
            }

            // Creare structura PKCS7
            sgn = new PdfPKCS7(null, certListX509, panelParams.hashAlgo, false);
            // Extragere octeti ce trebuiesc semnati
            byte[] sh = sgn.GetAuthenticatedAttributeBytes(documentHash, PdfSigner.CryptoStandard.CADES, null, null);

            return sh;
        }

        public bool addSignature(byte[] signHash, string signAlgo, bool selectedTimestamp)
        {
            // Atasarea semnaturii si a algoritmului de semnare
            sgn.SetExternalDigest(signHash, null, signAlgo);
            byte[] encodedSig = null;

            if (selectedTimestamp)
            {
                // Extragerea obiectului PKCS7 si adaugarea marcii temporale
                ITSAClient tsa = new TSAClientBouncyCastle("http://timestamp.digicert.com");
                encodedSig = sgn.GetEncodedPKCS7(documentHash, PdfSigner.CryptoStandard.CADES, tsa, null, null);
            }
            else
            {
                // Extragerea obiectului PKCS7, fara adaugare de marca temporala
                encodedSig = sgn.GetEncodedPKCS7(documentHash, PdfSigner.CryptoStandard.CADES, null, null, null);
            }


            string outFileName3 = Path.GetDirectoryName(docPathAux);
            outFileName3 += @"\";
            outFileName3 += Path.GetFileNameWithoutExtension(docPathAux);
            outFileName3 += "SIGNED";
            outFileName3 += Path.GetExtension(docPathAux);


            //FileStream outFile3 = new FileStream(outFileName3, FileMode.Create);

            using (FileStream outFile3 = new FileStream(outFileName3, FileMode.Create))
            {
                PdfReader reader3 = new PdfReader(tempFileName);
                PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());
                
                // Se creeaza un container in care este adaugat obiectul PKCS#7 codificat
                IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

                // Se ataseaza container-ul peste cel gol creat precedent
                PdfSigner.SignDeferred(signer3.GetDocument(), fieldName, outFile3, signature3);
                outFile3.Dispose();
            } 

            //File.Delete(tempFileName);

            return true;
        }

        private List<byte[]> docContent = new List<byte[]>();
        private List<byte[]> docHashes = new List<byte[]>();
        private List<string> outTempFileList = new List<string>();
        private List<PdfPKCS7> sgnMultiple = new List<PdfPKCS7>();
        private List<string> docPathMultipleAux = new List<string>();

        public List<byte[]> createMultipleSign(ParametersMultipleClass panelParams)
        {
            List<X509Certificate2> certList = new List<X509Certificate2>();
            foreach (string elem in panelParams.keyObject.cert.certificates)
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

            fieldName = panelParams.fieldName;

            int j = 0;
            foreach (string fileName in panelParams.docPath)
            {
                docPathMultipleAux.Add(fileName);

                string outTempFileName = Path.GetDirectoryName(fileName);
                Guid myuuid = Guid.NewGuid();
                outTempFileName += @"\";
                outTempFileName += myuuid.ToString();
                outTempFileName += ".TEMP";
                outTempFileList.Add(outTempFileName);

                PdfReader readerAux = new PdfReader(fileName);
                PdfDocument pdfDocument = new PdfDocument(readerAux);
                int pageNumberValue = pdfDocument.GetNumberOfPages();
                pdfDocument.Close();
                readerAux.Close();

                using (FileStream outFile2 = new FileStream(outTempFileName, FileMode.Create))
                {
                    PdfReader reader = new PdfReader(fileName);
                    PdfSigner signer = new PdfSigner(reader, outFile2, new StampingProperties());

                    PdfSignatureAppearance appearance2 = signer.GetSignatureAppearance();
                    appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(panelParams.castedXCoord, panelParams.castedYCoord, panelParams.castedWidthDist, panelParams.castedHeightDist));

                    if (panelParams.selectedPage == 1)
                        appearance2.SetPageNumber(1);
                    else
                    {
                        appearance2.SetPageNumber(pageNumberValue);
                    }
                    appearance2.SetCertificate(certListX509[0]);

                    if (panelParams.motiv != null)
                        appearance2.SetReason(panelParams.motiv);

                    if (panelParams.locatie != null)
                        appearance2.SetLocation(panelParams.locatie);

                    appearance2.SetSignatureCreator("iTextSharp7 with Bounty Castle");

                    // Signature Pad
                    if (panelParams.imageData != null)
                    {
                        ImageData image1 = ImageDataFactory.Create(panelParams.imageData);
                        appearance2.SetSignatureGraphic(image1);
                        appearance2.SetImageScale(1);
                        appearance2.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);
                    }

                    signer.SetFieldName(panelParams.fieldName);

                    PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED, panelParams.hashAlgo);
                    signer.SignExternalContainer(external, 16000);
                    byte[] documentHash = external.getHash();
                    docContent.Add(documentHash);
                }


                // Creare structura PKCS7
                PdfPKCS7 sgn_aux = new PdfPKCS7(null, certListX509, panelParams.hashAlgo, false);
                // Extragere octeti ce trebuiesc semnati
                byte[] sh_aux = sgn_aux.GetAuthenticatedAttributeBytes(docContent[j], PdfSigner.CryptoStandard.CADES, null, null);
                sgnMultiple.Add(sgn_aux);
                docHashes.Add(sh_aux);
                j++;
            }

            return docHashes;
        }


        public bool addMultipleSignature(List<byte[]> signHash, string signAlgo, bool selectedTimestamp)
        {
            int j = 0;
            foreach (string fileName in docPathMultipleAux)
            {
                sgnMultiple[j].SetExternalDigest(signHash[j], null, signAlgo);
                byte[] encodedSig = null;

                if (selectedTimestamp)
                {
                    ITSAClient tsa = new TSAClientBouncyCastle("http://timestamp.digicert.com");
                    //signer.Timestamp(tsa, "SignatureTimestamp");

                    encodedSig = sgnMultiple[j].GetEncodedPKCS7(docContent[j], PdfSigner.CryptoStandard.CADES, tsa, null, null);
                }
                else
                {
                    encodedSig = sgnMultiple[j].GetEncodedPKCS7(docContent[j], PdfSigner.CryptoStandard.CADES, null, null, null);
                }

                string outFileName3 = Path.GetDirectoryName(fileName);
                outFileName3 += @"\";
                outFileName3 += Path.GetFileNameWithoutExtension(fileName);
                outFileName3 += "SIGNED";
                outFileName3 += Path.GetExtension(fileName);

                using (FileStream outFile3 = new FileStream(outFileName3, FileMode.Create))
                {
                    PdfReader reader3 = new PdfReader(outTempFileList[j]);
                    PdfSigner signer3 = new PdfSigner(reader3, outFile3, new StampingProperties());

                    IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

                    PdfSigner.SignDeferred(signer3.GetDocument(), fieldName, outFile3, signature3);
                }

                j++;
            }
            

            //File.Delete(tempFileName);

            return true;
        }




    }
}
