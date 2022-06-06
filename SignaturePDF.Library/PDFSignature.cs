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

            tempFileName = outTempFileName;

            using (FileStream outFile2 = new FileStream(outTempFileName, FileMode.Create))
            {
                PdfReader reader = new PdfReader(panelParams.docPath);
                PdfSigner signer = new PdfSigner(reader, outFile2, new StampingProperties().UseAppendMode());

                PdfSignatureAppearance appearance2 = signer.GetSignatureAppearance();

                appearance2.SetPageRect(new iText.Kernel.Geom.Rectangle(panelParams.castedXCoord, panelParams.castedYCoord, panelParams.castedWidthDist, panelParams.castedHeightDist));

                if (panelParams.selectedPage == 0)
                    appearance2.SetPageNumber(1);
                else
                    appearance2.SetPageNumber(panelParams.selectedPage);

                appearance2.SetCertificate(certListX509[0]);

                if (panelParams.motiv != null)
                    appearance2.SetReason(panelParams.motiv);

                if (panelParams.locatie != null)
                    appearance2.SetLocation(panelParams.locatie);

                appearance2.SetSignatureCreator("iTextSharp7 with Bounty Castle");

                fieldName = panelParams.fieldName;
                signer.SetFieldName(panelParams.fieldName);

                PreSignatureContainer external = new PreSignatureContainer(PdfName.Adobe_PPKLite, PdfName.ETSI_CAdES_DETACHED, panelParams.hashAlgo);
                signer.SignExternalContainer(external, 16000);
                documentHash = external.getHash();
            }

            sgn = new PdfPKCS7(null, certListX509, panelParams.hashAlgo, false);
            byte[] sh = sgn.GetAuthenticatedAttributeBytes(documentHash, PdfSigner.CryptoStandard.CADES, null, null);

            return sh;
        }

        public bool addSignature(byte[] signHash, string signAlgo, bool selectedTimestamp)
        {
            sgn.SetExternalDigest(signHash, null, signAlgo);

            byte[] encodedSig = null;

            if (selectedTimestamp)
            {
                ITSAClient tsa = new TSAClientBouncyCastle("http://timestamp.digicert.com");
                //signer.Timestamp(tsa, "SignatureTimestamp");

                encodedSig = sgn.GetEncodedPKCS7(documentHash, PdfSigner.CryptoStandard.CADES, tsa, null, null);
            }
            else
            {
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

                IExternalSignatureContainer signature3 = new MyExternalSignatureContainer(encodedSig);

                PdfSigner.SignDeferred(signer3.GetDocument(), fieldName, outFile3, signature3);

            } 

            //File.Delete(tempFileName);

            return true;
        }




    }
}
