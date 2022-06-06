using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SignaturePDF.Library
{
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
}
