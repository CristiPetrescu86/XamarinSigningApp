using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class SignHashSendClass
    {
        public string credentialID { get; set; }

        public string SAD { get; set; }
        
        public List<string> hash { get; set; }

        public string hashAlgo { get; set; }

        public string signAlgo { get; set; }

        public string signAlgoParams { get; set; }

        public string clientData { get; set; }

        public SignHashSendClass()
        {
            hash = new List<string>();
        }

    }

    public class SignHashReceiveClass
    { 
        public List<string> signatures { get; set; }

        public SignHashReceiveClass()
        {
            signatures = new List<string>();
        }
    }

}
