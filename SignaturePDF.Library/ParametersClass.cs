using System;
using System.Collections.Generic;
using System.Text;

namespace SignaturePDF.Library
{
    public class ParametersClass
    {
        public string docPath;
        public CredentialsInfoReceiveClass keyObject;
        public float castedXCoord;
        public float castedYCoord;
        public float castedWidthDist;
        public float castedHeightDist;
        public int selectedPage;
        public string motiv;
        public string locatie;
        public string fieldName;
        public string hashAlgo;

        public ParametersClass(string docPath, CredentialsInfoReceiveClass credentialClass, float castedXCoord, float castedYCoord, float castedWidthDist, float castedHeightDist, int selectedPage, string motiv, string locatie, string hashAlgo, string fieldName)
        {
            this.docPath = docPath;
            this.keyObject = credentialClass;
            this.castedXCoord = castedXCoord;
            this.castedYCoord = castedYCoord;
            this.castedWidthDist = castedWidthDist;
            this.castedHeightDist = castedHeightDist;
            this.selectedPage = selectedPage;
            this.motiv = motiv;
            this.locatie = locatie;
            this.hashAlgo = hashAlgo;
            this.fieldName = fieldName;
        }

    }
}
