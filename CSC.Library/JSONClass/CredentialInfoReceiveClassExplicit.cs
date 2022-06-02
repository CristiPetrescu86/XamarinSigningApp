using System;
using System.Collections.Generic;
using System.Text;

namespace LicentaApp.JsonClass
{
    public class CredentialInfoReceiveClassExplicit
    {
        public string description { get; set; }

        public string credentialName { get; set; }

        public Key key { get; set; }

        public Certificates cert { get; set; }

        public string authMode { get; set; }
        public string SCAL { get; set; }

        public PIN_Class PIN { get; set; }

        public OTP_Class OTP { get; set; }

        public int multisign { get; set; }
        public string lang { get; set; }

    }


}
