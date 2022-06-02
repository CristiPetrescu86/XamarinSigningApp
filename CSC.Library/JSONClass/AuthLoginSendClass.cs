using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicentaApp.JsonClass
{
    public class AuthLoginSendClass
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string refresh_token { get; set; }
        public bool rememberMe { get; set; }
        public string clientData { get; set; }

    }
}
