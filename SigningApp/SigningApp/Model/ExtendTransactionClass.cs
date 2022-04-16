using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class ExtendTransactionSendClass
    {
        public string credentialID { get; set; }

        public string SAD { get; set; }

        public string clientData { get; set; }

    }


    public class ExtendTransactionReceiveClass
    {
        public string SAD { get; set; }

        public int expiresIn { get; set; } = 3600;
    }
}
