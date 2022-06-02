using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class CredentialsAuthorizeSendClass
    {
        public string credentialID { get; set; }

        public int numSignatures { get; set; }

        public List<string> hash { get; set; }

        public string PIN { get; set; }

        public string OTP { get; set; }


        public string description { get; set; }

        public string clientData { get; set; }

        public CredentialsAuthorizeSendClass()
        {
            hash = new List<string>();
        }
    }
}
