using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class SignatureTimestampSendClass
    {
        public string hash { get; set; }

        public string hashAlgo { get; set; }

        public string nonce { get; set; }

        public string clientData { get; set; }
    }

    public class SignatureTimestampReceiveClass
    {
        public string timestamp { get; set; }
    }
}
