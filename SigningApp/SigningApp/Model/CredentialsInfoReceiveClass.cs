using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class CredentialsInfoReceiveClass
    {
        public string description { get; set; }

        public string credentialName { get; set; }

        public Key key { get; set; }

        public Certificates cert { get; set; }
        
        public string authMode { get; set; }
        public string SCAL { get; set; }

        public PIN_Class PIN { get; set; }

        public OTP_Class OTP { get; set; }

        public int multisign { get; set; } = 1;
        public string lang { get; set; }


    }


    public class Key
    {
        // ------------------------------------------------ KEY PARAMS
        public string status { get; set; }

        public List<string> algo { get; set; }

        public int len { get; set; }

        public string curve { get; set; }


        public Key()
        {
            algo = new List<string>();
        }
    }

    public class Certificates
    {
        // ------------------------------------------------ CERTIFICATES PARAMS
        public string status { get; set; }
        public List<string> certificates { get; set; }
        public string issuerDN { get; set; }
        public string serialNumber { get; set; }
        public string subjectDN { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }

        public Certificates()
        {
            certificates = new List<string>();
        }
    }

    public class PIN_Class
    {
        // ------------------------------------------------ PIN PARAMS
        public string presence { get; set; }
        public string format { get; set; }
        public string label { get; set; }
        public string description { get; set; }
    }


    public class OTP_Class
    {
        // ------------------------------------------------ OTP PARAMS
        public string presence { get; set; }
        public string type { get; set; }
        public string format { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        public string ID { get; set; }
        public string provider { get; set; }
    }


}
