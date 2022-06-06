using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SignaturePDF.Library
{
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
}
